using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs;

public class CreateUserDocumentDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
    public string Title { get; set; }

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
    public string Category { get; set; }

    [Required(ErrorMessage = "La URL es obligatoria.")]
    [Url(ErrorMessage = "Debe ingresar una URL válida.")]
    public string Url { get; set; }

    [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse un usuario válido.")]
    public int User_id { get; set; }

    [Required(ErrorMessage = "El ID del consorcio es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe asociarse un consorcio válido.")]
    public int Consortium_id { get; set; }
}
