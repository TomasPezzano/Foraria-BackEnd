using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class ClaimResponseDto
{
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [MinLength(5, ErrorMessage = "La descripción debe tener al menos 5 caracteres")]
    public string Description { get; set; }

    [Required(ErrorMessage = "La fecha de respuesta es obligatoria")]
    public DateTime ResponseDate { get; set; }

    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public int User_id { get; set; }

    [Required(ErrorMessage = "El ID del reclamo es obligatorio")]
    public int Claim_id { get; set; }

    [Required(ErrorMessage = "El ID del sector es obligatorio")]
    public int ResponsibleSector_id { get; set; }
}
