
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.DTOs;

public class ApplyDiscountDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
}
