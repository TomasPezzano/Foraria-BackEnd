using Microsoft.AspNetCore.Http;

namespace ForariaDomain.Services;

public interface IOcrService
{
    Task<InvoiceOcrResult> ProcessInvoiceAsync(IFormFile file);
}

public class InvoiceOcrResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SupplierName { get; set; }
    public string? Cuit { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Description { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();

    public float ConfidenceScore { get; set; } 
}

public class InvoiceItem
{
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
