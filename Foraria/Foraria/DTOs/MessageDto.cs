using System;
using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
        public string? OptionalFile { get; set; }
        public int UserId { get; set; }
    }


    public class CreateMessageRequest
    {
        [Required(ErrorMessage = "El contenido del mensaje es obligatorio.")]
        [StringLength(1000, ErrorMessage = "El contenido no puede superar los 1000 caracteres.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "El hilo asociado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del hilo debe ser un número válido.")]
        public int Thread_id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser un número válido.")]
        public int User_id { get; set; }

        public string? optionalFile { get; set; }
    }

    public class MessageResponse
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
        public int Thread_id { get; set; }
        public int User_id { get; set; }
        public string? optionalFile { get; set; }
    }

}