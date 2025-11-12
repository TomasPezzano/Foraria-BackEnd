using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; }
}
