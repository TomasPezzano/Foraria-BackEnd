using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class CallCreateDto
{
    [Required]
    public int UserId { get; set; }
}
