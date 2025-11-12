using System.ComponentModel.DataAnnotations;

namespace Foraria.DTOs;

public class InvoiceRequestDto
{
    [Required(ErrorMessage = "El concepto es obligatorio")]
    [MaxLength(200)]
    public string Concept { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de factura es obligatorio")]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de emisión es obligatoria")]
    public DateTime DateOfIssue { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    public DateTime ExpirationDate { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El CUIT es obligatorio")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CUIT debe tener 11 dígitos")]
    public string Cuit { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "El subtotal no puede ser negativo")]
    public decimal SubTotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Los impuestos no pueden ser negativos")]
    public decimal TotalTaxes { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La ruta del archivo es obligatoria")]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SupplierAddress { get; set; }

    [MaxLength(100)]
    public string? PurchaseOrder { get; set; }

    public float? ConfidenceScore { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public List<InvoiceItemDto> Items { get; set; } = new();

    public int? ConsortiumId { get; set; }
}
