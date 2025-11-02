using System.ComponentModel.DataAnnotations;

namespace Foraria.Contracts.DTOs;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;
}
