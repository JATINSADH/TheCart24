
using System.ComponentModel.DataAnnotations;

namespace TheCart24.Api.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
