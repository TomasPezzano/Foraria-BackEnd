using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CommercialName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? BusinessName { get; set; }

    [Required]
    public string Cuit { get; set; } = string.Empty;

    // Relación con Categoría
    public int SupplierCategoryId { get; set; }
    public SupplierCategory SupplierCategory { get; set; }

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

    // Rating (calificación)
    [Range(0, 5)]
    public decimal? Rating { get; set; }

    public bool Active { get; set; } = true;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastInteraction { get; set; }

    // Relación con Contratos
    public ICollection<SupplierContract> Contracts { get; set; }

    // Multi-tenancy
    public int ConsortiumId { get; set; }
    public Consortium Consortium { get; set; } = null!;
}