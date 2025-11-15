using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class ReserveDto
{

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Debe especificar la fecha y hora de la reserva.")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Debe especificar el consorcio.")]
    public int Consortium_id { get; set; }

    [Required(ErrorMessage = "Debe especificar el lugar.")]
    public int Place_id { get; set; }

    [Required(ErrorMessage = "Debe especificar la residencia.")]
    public int Residence_id { get; set; }

    [Required(ErrorMessage = "Debe especificar el usuario que crea la reserva.")]
    public int User_id { get; set; }
}

public class ReserveResponseDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public int Place_id { get; set; }
    public string? PlaceName { get; set; }

    public int Residence_id { get; set; }
    public string? Residence { get; set; }

    public int User_id { get; set; }
    public string? UserName { get; set; }

    public DateTime DateReserve { get; set; }
}