using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class ClaimResponseDto
{
    public int Id { get; set; }

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

public class ClaimResponseResultDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime ResponseDate { get; set; }

    public int User_id { get; set; }
    public string? UserName { get; set; }

    public int Claim_id { get; set; }
    public string? ClaimTitle { get; set; }

    public int ResponsibleSector_id { get; set; }
    public string? ResponsibleSectorName { get; set; }
}