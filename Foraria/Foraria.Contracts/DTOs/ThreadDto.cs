using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class ThreadDto
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
        public int UserId { get; set; }
        public int ForumId { get; set; }
    }
    public class CreateThreadRequest
    {
        [Required(ErrorMessage = "El tema del hilo es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El tema debe tener entre 3 y 100 caracteres.")]
        public string Theme { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El foro asociado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del foro debe ser un número válido.")]
        public int ForumId { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser un número válido.")]
        public int UserId { get; set; }
    }

    public class UpdateThreadRequest
    {
        [MaxLength(200, ErrorMessage = "El tema no puede superar los 200 caracteres.")]
        public string? Theme { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Description { get; set; }

        [RegularExpression("Active|Closed|Archived", ErrorMessage = "El estado debe ser Active, Closed o Archived.")]
        public string? State { get; set; }
    }

    public class ThreadResponse
    {
        public int Id { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
        public int Forum_id { get; set; }
        public int User_id { get; set; }
    }
}

   

