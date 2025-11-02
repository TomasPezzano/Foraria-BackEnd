using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class UpdatePollRequest
    {
        [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string? Title { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Description { get; set; }

        [RegularExpression("Pendiente|Borrador|Activa|Rechazada|Cerrada",
            ErrorMessage = "El estado debe ser Pendiente, Borrador, Activa, Rechazada o Cerrada.")]
        public string? State { get; set; }

        public int UserId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
