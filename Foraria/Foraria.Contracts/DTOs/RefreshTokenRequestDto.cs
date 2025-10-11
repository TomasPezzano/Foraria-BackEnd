using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; }
}
