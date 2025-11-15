using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class CallJoinDto
{
    [Required]
    public int UserId { get; set; }
}
