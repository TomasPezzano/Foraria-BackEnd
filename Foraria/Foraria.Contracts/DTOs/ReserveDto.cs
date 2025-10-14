using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class ReserveRequestDto
{

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Debe especificar la fecha y hora de la reserva.")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Debe especificar el lugar.")]
    public int Place_id { get; set; }

    [Required(ErrorMessage = "Debe especificar la residencia.")]
    public int Residence_id { get; set; }

    [Required(ErrorMessage = "Debe especificar el usuario que crea la reserva.")]
    public int User_id { get; set; }
}