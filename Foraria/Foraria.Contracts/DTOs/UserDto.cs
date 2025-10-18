using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class UserDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nombre es requerido")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Apellido es requerido")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Numero es requerido")]
    public long PhoneNumber { get; set; }

    [Required(ErrorMessage = "Rol es requerido")]
    public int RoleId { get; set; }

    public string TemporaryPassword { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }
    public List<ResidenceDto>? Residences { get; set; }

}
