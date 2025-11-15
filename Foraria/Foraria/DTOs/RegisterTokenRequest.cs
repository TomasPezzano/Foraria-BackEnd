using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class RegisterTokenRequest
{
    [Required(ErrorMessage = "El FCM token es requerido")]
    [MaxLength(500)]
    public string FcmToken { get; set; } = string.Empty;
}
