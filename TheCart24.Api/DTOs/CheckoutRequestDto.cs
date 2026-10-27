
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.DTOs;

public class CheckoutRequestDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string AddressLine { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    [RegularExpression(@"^[A-Za-z0-9 -]{3,10}$")]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(COD|Prepaid)$",
    ErrorMessage = "Payment method must be COD or Prepaid.")]
    public string PaymentMethod { get; set; } = "COD";
    public bool ApplyDiscount { get; set; } = false;
}
