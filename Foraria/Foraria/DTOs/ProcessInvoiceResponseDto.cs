namespace Foraria.DTOs;

public class ProcessInvoiceResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SupplierName { get; set; }
    public string? Cuit { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalTax { get; set; }
    public string? SupplierAddress { get; set; }
    public string? PurchaseOrder { get; set; }
    public string? Description { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public string? FilePath { get; set; }
    public float ConfidenceScore { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class InvoiceItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
