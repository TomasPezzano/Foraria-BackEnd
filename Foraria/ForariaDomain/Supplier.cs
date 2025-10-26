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


    public string CommercialName { get; set; } = string.Empty;


    public string? BusinessName { get; set; }

    public string Cuit { get; set; } = string.Empty;

    public string SupplierCategory { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? ContactPerson { get; set; }

    public string? Observations { get; set; }

    // Rating (calificación)
    public decimal? Rating { get; set; }

    public bool Active { get; set; } = true;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastInteraction { get; set; }

    // Relación con Contratos
    public ICollection<SupplierContract?> Contracts { get; set; }


    public int ConsortiumId { get; set; }
    [ForeignKey(nameof(ConsortiumId))]
    public Consortium Consortium { get; set; } = null!;
}