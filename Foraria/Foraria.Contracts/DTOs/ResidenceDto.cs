using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class ResidenceDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Numero es requerido")]
    public int Number { get; set; }

    [Required(ErrorMessage = "Piso es requerido")]
    public int Floor { get; set; }

    [Required(ErrorMessage = "Torre es requerida")]
    public string Tower { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; } = string.Empty;

    public int? ConsortiumId { get; set; }

}
