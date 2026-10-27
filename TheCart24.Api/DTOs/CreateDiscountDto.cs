
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.DTOs;

public class CreateDiscountDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(0.01, 100)]
    public decimal DiscountPercentage { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
