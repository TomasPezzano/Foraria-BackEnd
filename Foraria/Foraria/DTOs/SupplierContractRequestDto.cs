using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class SupplierContractRequestDto
{
    [Required(ErrorMessage = "El nombre del contrato es obligatorio.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContractType { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El monto mensual es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto mensual debe ser mayor a 0.")]
    public decimal MonthlyAmount { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "El ID del proveedor es obligatorio.")]
    public int SupplierId { get; set; }
}
