using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class InvoiceItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }  

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal? Amount { get; set; }  

    public int? Quantity { get; set; } 

    [Precision(18, 2)]
    public decimal? UnitPrice { get; set; }  

  
    [ForeignKey(nameof(InvoiceId))]
    public Invoice Invoice { get; set; } = null!;
}
