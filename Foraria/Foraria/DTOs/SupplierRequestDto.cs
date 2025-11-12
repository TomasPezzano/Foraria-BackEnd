using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class SupplierRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CommercialName { get; set; }

    [MaxLength(200)]
    public string BusinessName { get; set; }

    [Required]
    public string Cuit { get; set; } = string.Empty;

    [Required]
    public string supplierCategory { get; set; }


    [EmailAddress(ErrorMessage = "Debe ser email")]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(1000)]
    public string? Observations { get; set; }

    [Required(ErrorMessage = "El ConsortiumId es obligatorio.")]
    public int ConsortiumId { get; set; }

}
