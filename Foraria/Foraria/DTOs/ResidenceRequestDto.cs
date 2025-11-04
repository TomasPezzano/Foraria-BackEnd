using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class ResidenceRequestDto
{
    [Required(ErrorMessage = "Numero es requerido")]
    public int Number { get; set; }

    [Required(ErrorMessage = "Piso es requerido")]
    public int Floor { get; set; }

    [Required(ErrorMessage = "Torre es requerida")]
    public string Tower { get; set; }

    public int ConsortiumId { get; set; }
}
