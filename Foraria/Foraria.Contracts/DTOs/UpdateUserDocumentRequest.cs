using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class UpdateUserDocumentRequest
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int UserId { get; set; }

        [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public string? Title { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
        public string? Category { get; set; }

        [Url(ErrorMessage = "Debe ingresar una URL válida.")]
        public string? Url { get; set; }
    }
}
