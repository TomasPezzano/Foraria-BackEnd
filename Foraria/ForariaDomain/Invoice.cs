using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain;

public class Invoice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Concept { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfIssue { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal Amount { get; set; }  // Total con impuestos

    [Required]
    [MaxLength(11)]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CUIT debe tener 11 dígitos")]
    public string Cuit { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Precision(18, 2)]
    public decimal TotalTaxes { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SupplierAddress { get; set; }  

    [MaxLength(100)]
    public string? PurchaseOrder { get; set; } 

    public float? ConfidenceScore { get; set; } 

    public DateTime ProcessedAt { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    public int? ExpenseId { get; set; }

    public Expense? Expense { get; set;}

    public int? ConsortiumId { get; set; }

    public Consortium? Consortium { get; set; }
}

