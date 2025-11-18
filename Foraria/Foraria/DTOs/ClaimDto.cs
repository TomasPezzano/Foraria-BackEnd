using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class ClaimDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres")]
    public string Title { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
    public string Description { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria")]
    [RegularExpression("Alta|Media|Baja|Urgente", ErrorMessage = "La prioridad debe ser: Alta, Media o Baja")]
    public string Priority { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres")]
    public string Category { get; set; }
    public string? Archive { get; set; }
    public int? User_id { get; set; }

    public int ResidenceId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? State { get; set; }

    public int ConsortiumId { get; set; }
}
