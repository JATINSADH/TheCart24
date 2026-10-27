
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, 10000000)]
    public decimal UnitPrice { get; set; }

    [Range(1, 100000)]
    public int Quantity { get; set; }

    public decimal Subtotal => UnitPrice * Quantity;

    public Order? Order { get; set; }

    public Product? Product { get; set; }
    public string? SellerId { get; set; }
    [Required]
    [MaxLength(30)]
    public string SellerStatus { get; set; } = "Pending";
}
