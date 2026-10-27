
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Range(0, 100000000)]
    public decimal TotalAmount { get; set; }

    [Range(0, 100000000)]
    public decimal Subtotal { get; set; }

    [Range(0, 100000000)]
    public decimal DiscountAmount { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public List<OrderItem> Items { get; set; } = new();
    [MaxLength(100)]
    public string? CheckoutKey { get; set; }


    [Required]
    [MaxLength(150)]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string ShippingAddressLine { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ShippingState { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = "COD";

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

}
