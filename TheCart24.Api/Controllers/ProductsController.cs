
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheCart24.Api.Data;
using TheCart24.Api.DTOs;
using TheCart24.Api.Models;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    TheCart24DbContext context,
    UserManager<IdentityUser> userManager) : ControllerBase
{
    // Public: sabhi visitors products dekh sakte hain.

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "newest",
        int page = 1,
        int pageSize = 10)
    {
        if (minPrice < 0 || maxPrice < 0 ||
            (minPrice.HasValue && maxPrice.HasValue &&
             minPrice > maxPrice))
        {
            return BadRequest(new
            {
                message = "Invalid price range."
            });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "Page must be >= 1 and pageSize must be between 1 and 100."
            });
        }

        var query = context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Description.Contains(term));
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        query = sortBy.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.Price)
                                .ThenBy(p => p.Id),

            "price_desc" => query.OrderByDescending(p => p.Price)
                                 .ThenBy(p => p.Id),

            "name_asc" => query.OrderBy(p => p.Name)
                               .ThenBy(p => p.Id),

            "oldest" => query.OrderBy(p => p.CreatedAt)
                             .ThenBy(p => p.Id),

            "newest" => query.OrderByDescending(p => p.CreatedAt)
                             .ThenByDescending(p => p.Id),

            _ => query.OrderByDescending(p => p.CreatedAt)
                      .ThenByDescending(p => p.Id)
        };

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            products
        });
    }


    // Sirf Admin aur SuperAdmin product create kar sakte hain.
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateProduct(
        CreateProductDto dto)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "User ID not found in the token."
            });
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Authenticated user was not found."
            });
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl,
            SellerId = user.Id
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetProducts),
            new { id = product.Id },
            response);
    }


    // Sirf logged-in seller ke products
    [HttpGet("my")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetMyProducts()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var products = await context.Products
            .AsNoTracking()
            .Where(p => p.SellerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(products);
    }

    // Seller sirf apna product update kar sakta hai
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateProduct(
        int id, CreateProductDto dto)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound(new { message = "Product not found." });

        if (product.SellerId != userId)
            return Forbid();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ImageUrl = dto.ImageUrl;

        await context.SaveChangesAsync();

        return Ok(new { message = "Product updated successfully." });
    }

    // Seller sirf apna product delete kar sakta hai
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound(new { message = "Product not found." });

        if (product.SellerId != userId)
            return Forbid();

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully." });
    }


    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }


}
