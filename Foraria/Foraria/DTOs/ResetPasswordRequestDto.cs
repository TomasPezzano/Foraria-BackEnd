using System.ComponentModel.DataAnnotations;


namespace Foraria.DTOs;
public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "El token es requerido")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmar contraseña es requerido")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
