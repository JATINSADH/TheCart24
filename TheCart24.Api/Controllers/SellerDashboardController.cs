
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheCart24.Api.Data;
using TheCart24.Api.DTOs;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/seller-dashboard")]
[Authorize(Roles = "Admin")]
public class SellerDashboardController(
    TheCart24DbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var sellerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sellerId))
            return Unauthorized();

        var totalProducts = await context.Products
            .CountAsync(p => p.SellerId == sellerId);

        var sellerOrderItems = context.OrderItems
            .Where(i => i.SellerId == sellerId);

        var orderIds = sellerOrderItems
            .Select(i => i.OrderId)
            .Distinct();

        var orders = context.Orders
            .Where(o => orderIds.Contains(o.Id));

        var dashboard = new SellerDashboardDto
        {
            TotalProducts = totalProducts,

            TotalOrders = await orders.CountAsync(),

            PendingOrders = await orders.CountAsync(
                o => o.Status == "Pending"),

            ShippedOrders = await orders.CountAsync(
                o => o.Status == "Shipped"),

            DeliveredOrders = await orders.CountAsync(
                o => o.Status == "Delivered"),

            TotalSales = await sellerOrderItems
                .Where(i => i.Order != null &&
                    i.Order.Status != "Cancelled")
                .SumAsync(i =>
                    (decimal?)(i.UnitPrice * i.Quantity)) ?? 0
        };

        return Ok(dashboard);
    }
}
