
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
[Authorize(Roles = "User,Admin,SuperAdmin")]
public class OrdersController(
    TheCart24DbContext context) : ControllerBase
{


    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
    [FromHeader(Name = "Idempotency-Key")] string? checkoutKey,
    [FromBody] CheckoutRequestDto shipping)
    {
        // 1. Validate the logged-in user first
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        // 2. Read and validate the idempotency key
        if (string.IsNullOrWhiteSpace(checkoutKey))
        {
            return BadRequest(new
            {
                message = "Idempotency-Key header is required."
            });
        }

        checkoutKey = checkoutKey.Trim();

        if (string.IsNullOrWhiteSpace(checkoutKey))
        {
            return BadRequest(new
            {
                message = "Idempotency-Key header is required."
            });
        }

        checkoutKey = checkoutKey.Trim();

        if (checkoutKey.Length > 100)
        {
            return BadRequest(new
            {
                message = "Idempotency-Key cannot exceed 100 characters."
            });
        }

        // 3. Return the existing order if this key was already used
        var existingOrder = await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId &&
                        o.CheckoutKey == checkoutKey)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Subtotal = o.Subtotal,
                DiscountAmount = o.DiscountAmount,
                Items = o.Items.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (existingOrder is not null)
            return Ok(existingOrder);

        // 4. Start the checkout transaction
        await using var transaction =
            await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var cartItems = await context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest(new
            {
                message = "Your cart is empty."
            });
        }

        // 5. Validate products and stock
        foreach (var item in cartItems)
        {
            if (item.Product is null)
            {
                return BadRequest(new
                {
                    message = $"Product {item.ProductId} is unavailable."
                });
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                return BadRequest(new
                {
                    message = $"Insufficient stock for {item.Product.Name}."
                });
            }
        }


        var productIds = cartItems
         .Select(item => item.ProductId)
         .Distinct()
         .ToList();

        var subtotal = cartItems.Sum(item =>
            item.Product!.Price * item.Quantity);

        var discountPercentage = 0m;

        // Discount tabhi calculate hoga jab customer Apply Discount karega.
        if (shipping.ApplyDiscount)
        {
            var now = DateTime.UtcNow;

            discountPercentage = await context.Discounts
                .AsNoTracking()
                .Where(d =>
                    productIds.Contains(d.ProductId) &&
                    d.IsActive &&
                    d.StartDate <= now &&
                    d.EndDate > now)
                .Select(d => d.DiscountPercentage)
                .DefaultIfEmpty(0m)
                .MaxAsync();
        }

        var discountAmount = Math.Round(
            subtotal * discountPercentage / 100m,
            2,
            MidpointRounding.AwayFromZero);

        var finalTotal = subtotal - discountAmount;

        // 6. Create the order with its idempotency key
        var order = new Order
        {
            UserId = userId,
            CheckoutKey = checkoutKey,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",

            PaymentMethod = shipping.PaymentMethod,
            PaymentStatus = "Pending",

            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TotalAmount = finalTotal,

            ShippingFullName = shipping.FullName.Trim(),
            ShippingPhone = shipping.Phone.Trim(),
            ShippingAddressLine = shipping.AddressLine.Trim(),
            ShippingCity = shipping.City.Trim(),
            ShippingState = shipping.State.Trim(),
            ShippingPostalCode = shipping.PostalCode.Trim()
        };

        foreach (var cartItem in cartItems)
        {
            var product = cartItem.Product!;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                SellerId = product.SellerId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                SellerStatus = "Pending",

            });

            product.StockQuantity -= cartItem.Quantity;
            
        }


        context.Orders.Add(order);
        context.CartItems.RemoveRange(cartItems);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            // Discard the failed attempt's tracked entities.
            context.ChangeTracker.Clear();

            // If another concurrent request created this order
            // with the same key, return that existing order.
            var duplicateOrder = await context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId &&
                            o.CheckoutKey == checkoutKey)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,

                    ShippingFullName = o.ShippingFullName,
                    ShippingPhone = o.ShippingPhone,
                    ShippingAddressLine = o.ShippingAddressLine,
                    ShippingCity = o.ShippingCity,
                    ShippingState = o.ShippingState,
                    ShippingPostalCode = o.ShippingPostalCode,

                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,

                    Items = o.Items.Select(i => new OrderItemResponseDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        Subtotal = i.UnitPrice * i.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (duplicateOrder is not null)
                return Ok(duplicateOrder);

            // It was a different database error, not a duplicate checkout.
            throw;
        }


        // 7. Return the created order
        var response = new OrderResponseDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,

            ShippingFullName = order.ShippingFullName,
            ShippingPhone = order.ShippingPhone,
            ShippingAddressLine = order.ShippingAddressLine,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,

            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,

            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        return Ok(response);
    }


    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var orders = await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,

                ShippingFullName = o.ShippingFullName,
                ShippingPhone = o.ShippingPhone,
                ShippingAddressLine = o.ShippingAddressLine,
                ShippingCity = o.ShippingCity,
                ShippingState = o.ShippingState,
                ShippingPostalCode = o.ShippingPostalCode,

                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,

                Items = o.Items.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity,
                    SellerStatus = i.SellerStatus
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }


    [HttpGet("seller")]
    public async Task<IActionResult> GetSellerOrders()
    {
        var sellerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sellerId))
            return Unauthorized();

        var orders = await context.OrderItems
            .AsNoTracking()
            .Where(item => item.SellerId == sellerId)
            .GroupBy(item => item.OrderId)
            .Select(group => new
            {
                OrderId = group.Key,
                Items = group.Select(item => new
                {
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    Subtotal = item.UnitPrice * item.Quantity
                }).ToList(),
                SellerTotal = group.Sum(
                    item => item.UnitPrice * item.Quantity)
            })
            .ToListAsync();

        return Ok(orders);
    }



    [HttpPatch("{orderId:int}/seller-status")]
    public async Task<IActionResult> UpdateSellerOrderStatus(
        int orderId,
        UpdateOrderStatusDto dto)
    {
        var sellerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sellerId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new
            {
                message = "Status is required."
            });
        }

        var newStatus = dto.Status.Trim();

        var allowedStatuses = new[]
        {
        "Confirmed",
        "Shipped",
        "Delivered"
    };

        var normalizedStatus = allowedStatuses.FirstOrDefault(
            status => status.Equals(
                newStatus, StringComparison.OrdinalIgnoreCase));

        if (normalizedStatus is null)
        {
            return BadRequest(new
            {
                message = "Allowed statuses: Confirmed, Shipped, Delivered."
            });
        }

        var sellerItems = await context.OrderItems
            .Where(item =>
                item.OrderId == orderId &&
                item.SellerId == sellerId)
            .ToListAsync();

        if (sellerItems.Count == 0)
        {
            var orderExists = await context.Orders
                .AnyAsync(order => order.Id == orderId);

            if (!orderExists)
                return NotFound(new { message = "Order not found." });

            return Forbid();
        }

        // All of this seller's items must be at the same stage
        // before advancing to the next stage.
        var valid = sellerItems.All(item =>
            (item.SellerStatus == "Pending" &&
                normalizedStatus == "Confirmed") ||
            (item.SellerStatus == "Confirmed" &&
                normalizedStatus == "Shipped") ||
            (item.SellerStatus == "Shipped" &&
                normalizedStatus == "Delivered"));

        if (!valid)
        {
            return BadRequest(new
            {
                message = "Invalid status transition. Allowed flow: Pending -> Confirmed -> Shipped -> Delivered."
            });
        }

        foreach (var item in sellerItems)
        {
            item.SellerStatus = normalizedStatus;
        }

        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Seller order items updated successfully.",
            orderId,
            updatedItems = sellerItems.Count,
            status = normalizedStatus
        });
    }



    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await using var transaction =
            await context.Database.BeginTransactionAsync();

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId && o.UserId == userId);

        if (order is null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        if (order.Status != "Pending")
        {
            return BadRequest(new
            {
                message = "Only Pending orders can be cancelled."
            });
        }

        var productIds = order.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                product.StockQuantity += item.Quantity;
            }
        }

        order.Status = "Cancelled";

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Order cancelled successfully.",
            orderId = order.Id,
            status = order.Status
        });
    }


}
