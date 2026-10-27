
namespace TheCart24.Api.DTOs;

public class SellerDashboardDto
{
    public int TotalProducts { get; set; }

    public int TotalOrders { get; set; }

    public int PendingOrders { get; set; }

    public int ShippedOrders { get; set; }

    public int DeliveredOrders { get; set; }

    public decimal TotalSales { get; set; }
}
