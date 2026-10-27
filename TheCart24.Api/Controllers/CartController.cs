
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheCart24.Api.Data;
using TheCart24.Api.DTOs;
using TheCart24.Api.Models;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// checks tha t the user have permission or not 
[Authorize(Roles = "User,Admin,SuperAdmin")]
public class CartController(
    TheCart24DbContext context) : ControllerBase
{
    
    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = CurrentUserId;

        // authenicated ,  but who is user this checks that come from JWT token
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var items = await context.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new CartItemResponseDto
            {
                CartItemId = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product!.Name,
                Price = c.Product.Price,
                Quantity = c.Quantity,
                Subtotal = c.Product.Price * c.Quantity,
                ImageUrl = c.Product.ImageUrl
            })
            .ToListAsync();

        return Ok(new
        {
            items,
            total = items.Sum(i => i.Subtotal)
        });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(AddToCartDto dto)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (dto.Quantity < 1 || dto.Quantity > 100)
            return BadRequest(new
            {
                message = "Quantity must be between 1 and 100."
            });

        await using var transaction =
            await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product is null)
                return NotFound(new { message = "Product not found." });

            var item = await context.CartItems.FirstOrDefaultAsync(
                c => c.UserId == userId &&
                     c.ProductId == dto.ProductId);

            var newQuantity = (item?.Quantity ?? 0) + dto.Quantity;

            if (newQuantity > 100)
                return BadRequest(new
                {
                    message = "Maximum cart quantity is 100."
                });

            // Stock tabhi reduce hoga jab requested quantity available ho.
            var affectedRows = await context.Products
                .Where(p =>
                    p.Id == dto.ProductId &&
                    p.StockQuantity >= dto.Quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        p => p.StockQuantity,
                        p => p.StockQuantity - dto.Quantity));

            if (affectedRows == 0)
                return Conflict(new
                {
                    message = "Insufficient stock. Please refresh and try again."
                });

            if (item is null)
            {
                item = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                context.CartItems.Add(item);
            }
            else
            {
                item.Quantity = newQuantity;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Product added and stock reserved.",
                productId = product.Id,
                quantity = newQuantity
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateQuantity(
        int cartItemId,
        [FromBody] int quantity)
    {
        if (quantity < 1 || quantity > 100)
            return BadRequest(new
            {
                message = "Quantity must be between 1 and 100."
            });

        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await using var transaction =
            await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var item = await context.CartItems.FirstOrDefaultAsync(c =>
                c.Id == cartItemId && c.UserId == userId);

            if (item is null)
                return NotFound(new { message = "Cart item not found." });

            var difference = quantity - item.Quantity;

            if (difference > 0)
            {
                // Increase quantity: extra stock reserve karo.
                var affectedRows = await context.Products
                    .Where(p =>
                        p.Id == item.ProductId &&
                        p.StockQuantity >= difference)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            p => p.StockQuantity,
                            p => p.StockQuantity - difference));

                if (affectedRows == 0)
                    return Conflict(new
                    {
                        message = "Insufficient stock for this quantity."
                    });
            }
            else if (difference < 0)
            {
                // Decrease quantity: unused reservation release karo.
                var quantityToRelease = -difference;

                await context.Products
                    .Where(p => p.Id == item.ProductId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            p => p.StockQuantity,
                            p => p.StockQuantity + quantityToRelease));
            }

            item.Quantity = quantity;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Cart quantity updated.",
                quantity
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await using var transaction =
            await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var item = await context.CartItems.FirstOrDefaultAsync(c =>
                c.Id == cartItemId && c.UserId == userId);

            if (item is null)
                return NotFound(new { message = "Cart item not found." });

            var quantityToRelease = item.Quantity;
            var productId = item.ProductId;

            await context.Products
                .Where(p => p.Id == productId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        p => p.StockQuantity,
                        p => p.StockQuantity + quantityToRelease));

            context.CartItems.Remove(item);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Product removed and reserved stock released."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
