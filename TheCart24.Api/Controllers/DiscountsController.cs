
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheCart24.Api.Data;
using TheCart24.Api.DTOs;
using TheCart24.Api.Models;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController(
    TheCart24DbContext context) : ControllerBase
{
    // Admin only: create a discount
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDiscount(
        CreateDiscountDto dto)
    {
        if (dto.StartDate >= dto.EndDate)
        {
            return BadRequest(new
            {
                message = "EndDate must be later than StartDate."
            });
        }

        var productExists = await context.Products
            .AnyAsync(p => p.Id == dto.ProductId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var discount = new Discount
        {
            Name = dto.Name.Trim(),
            ProductId = dto.ProductId,
            DiscountPercentage = dto.DiscountPercentage,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Discounts.Add(discount);
        await context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetDiscountById),
            new { id = discount.Id },
            new
            {
                discount.Id,
                discount.Name,
                discount.ProductId,
                discount.DiscountPercentage,
                discount.StartDate,
                discount.EndDate,
                discount.IsActive
            });
    }

    // Admin only: list all discounts
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDiscounts()
    {
        var discounts = await context.Discounts
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.ProductId,
                ProductName = d.Product.Name,
                d.DiscountPercentage,
                d.StartDate,
                d.EndDate,
                d.IsActive,
                d.CreatedAt
            })
            .ToListAsync();

        return Ok(discounts);
    }

    // Admin only: get one discount
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDiscountById(int id)
    {
        var discount = await context.Discounts
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.ProductId,
                ProductName = d.Product.Name,
                d.DiscountPercentage,
                d.StartDate,
                d.EndDate,
                d.IsActive,
                d.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (discount is null)
        {
            return NotFound(new
            {
                message = "Discount not found."
            });
        }

        return Ok(discount);
    }

    // Public: calculate the current discounted price
    [HttpPost("apply")]
    [AllowAnonymous]
    public async Task<IActionResult> ApplyDiscount(
        ApplyDiscountDto dto)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var now = DateTime.UtcNow;

        var discount = await context.Discounts
            .AsNoTracking()
            .Where(d =>
                d.ProductId == dto.ProductId &&
                d.IsActive &&
                d.StartDate <= now &&
                d.EndDate > now)
            .OrderByDescending(d => d.DiscountPercentage)
            .FirstOrDefaultAsync();

        if (discount is null)
        {
            return Ok(new
            {
                product.Id,
                product.Name,
                OriginalPrice = product.Price,
                DiscountPercentage = 0m,
                DiscountAmount = 0m,
                DiscountedPrice = product.Price,
                Message = "No active discount for this product."
            });
        }

        var discountAmount = Math.Round(
            product.Price * discount.DiscountPercentage / 100m,
            2,
            MidpointRounding.AwayFromZero);

        return Ok(new
        {
            product.Id,
            product.Name,
            OriginalPrice = product.Price,
            discount.DiscountPercentage,
            DiscountAmount = discountAmount,
            DiscountedPrice = product.Price - discountAmount,
            DiscountName = discount.Name
        });
    }

    // Admin only: delete a discount
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        var discount = await context.Discounts
            .FirstOrDefaultAsync(d => d.Id == id);

        if (discount is null)
        {
            return NotFound(new
            {
                message = "Discount not found."
            });
        }

        context.Discounts.Remove(discount);
        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Discount deleted successfully."
        });
    }

    [HttpGet("announcement")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscountAnnouncement()
    {
        var now = DateTime.UtcNow;

        var discount = await context.Discounts
            .AsNoTracking()
            .Where(d =>
                d.IsActive &&
                d.StartDate <= now &&
                d.EndDate > now)
            .OrderByDescending(d => d.DiscountPercentage)
            .Select(d => new
            {
                d.Name,
                d.DiscountPercentage,
                d.StartDate,
                d.EndDate
            })
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return NotFound(new
            {
                message = "No active offers right now."
            });
        }

        return Ok(discount);
    }
}
