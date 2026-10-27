
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.DTOs;

public class AddToCartDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
