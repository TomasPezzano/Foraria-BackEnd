using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class ReactionRequest
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del usuario debe ser válido.")]
        public int User_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del mensaje debe ser válido.")]
        public int? Message_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del hilo debe ser válido.")]
        public int? Thread_id { get; set; }
        public int ReactionType { get; set; } // +1 like -1 dislike
    }
}