using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.DTOs
{
    public class ChangePollStateRequest
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
        [RegularExpression("Activa|Rechazada", ErrorMessage = "El estado debe ser Activa o Rechazada.")]
        public string NewState { get; set; }
    }
}
