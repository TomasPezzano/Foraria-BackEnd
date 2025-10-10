using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class RegisterUserRequestDto
{
    [Required(ErrorMessage = "Nombre completo es requerido")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Nombre completo es requerido")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Numero es requerido")]
    [Phone(ErrorMessage = "Formato de numero inválido")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Rol es requerido")]
    public int RoleId { get; set; }

    public List<ResidenceDto>? Residences { get; set; }


}
