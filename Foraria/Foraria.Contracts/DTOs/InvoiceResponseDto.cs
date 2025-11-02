using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Contracts.DTOs;

public class InvoiceResponseDto
{
    public int Id { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime DateOfIssue { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal Amount { get; set; }
    public string Cuit { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TotalTaxes { get; set; }
    public string? Description { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? SupplierAddress { get; set; }
    public string? PurchaseOrder { get; set; }
    public float? ConfidenceScore { get; set; }
    public DateTime ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? ConsortiumId { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}
