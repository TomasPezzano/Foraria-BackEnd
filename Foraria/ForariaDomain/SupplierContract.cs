using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class SupplierContract
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContractType { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool Active { get; set; } = true;

    // Archivo del contrato (PDF, etc.)
    public string? FilePath { get; set; }

    // Relación con Proveedor
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}