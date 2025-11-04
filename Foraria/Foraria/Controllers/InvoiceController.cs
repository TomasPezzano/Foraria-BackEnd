using Foraria.Contracts.DTOs;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    public readonly ICreateInvoice _createInvoice;
    public readonly IGetAllInvoices _getAllInvoices;
    public InvoiceController(ICreateInvoice createInvoice, IGetAllInvoices getAllInvoices)
    {
        _createInvoice = createInvoice;
        _getAllInvoices = getAllInvoices;
    }

    [HttpPost]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto invoiceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var invoice = new Invoice
        {
            Concept = invoiceDto.Concept,
            Category = invoiceDto.Category,
            InvoiceNumber = invoiceDto.InvoiceNumber,
            SupplierName = invoiceDto.SupplierName,
            DateOfIssue = invoiceDto.DateOfIssue,
            ExpirationDate = invoiceDto.ExpirationDate,
            Amount = invoiceDto.Amount,
            Cuit = invoiceDto.Cuit,
            SubTotal = invoiceDto.SubTotal,
            TotalTaxes = invoiceDto.TotalTaxes,
            Description = invoiceDto.Description,
            FilePath = invoiceDto.FilePath,
            SupplierAddress = invoiceDto.SupplierAddress,
            PurchaseOrder = invoiceDto.PurchaseOrder,
            ConfidenceScore = invoiceDto.ConfidenceScore,
            ProcessedAt = invoiceDto.ProcessedAt ?? DateTime.UtcNow,
            ConsortiumId = invoiceDto.ConsortiumId,
            Items = invoiceDto.Items.Select(itemDto => new InvoiceItem
            {
                Description = itemDto.Description,
                Amount = itemDto.Amount,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice
            }).ToList()
        };

        try
        {
            var result = await _createInvoice.Execute(invoice);

            var response = new InvoiceResponseDto
            {
                Id = result.Id,
                Concept = result.Concept,
                Category = result.Category,
                InvoiceNumber = result.InvoiceNumber,
                SupplierName = result.SupplierName,
                DateOfIssue = result.DateOfIssue,
                ExpirationDate = result.ExpirationDate,
                Amount = result.Amount,
                Cuit = result.Cuit,
                SubTotal = result.SubTotal,
                TotalTaxes = result.TotalTaxes,
                Description = result.Description,
                FilePath = result.FilePath,
                SupplierAddress = result.SupplierAddress,
                PurchaseOrder = result.PurchaseOrder,
                ConfidenceScore = result.ConfidenceScore,
                ProcessedAt = result.ProcessedAt,
                ConsortiumId = result.ConsortiumId,
                Items = result.Items.Select(item => new InvoiceItemDto
                {
                    Description = item.Description,
                    Amount = item.Amount,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la factura", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _getAllInvoices.Execute();
        var response = invoices.Select(result => new InvoiceResponseDto
        {
            Id = result.Id,
            Concept = result.Concept,
            Category = result.Category,
            InvoiceNumber = result.InvoiceNumber,
            SupplierName = result.SupplierName,
            DateOfIssue = result.DateOfIssue,
            ExpirationDate = result.ExpirationDate,
            Amount = result.Amount,
            Cuit = result.Cuit,
            SubTotal = result.SubTotal,
            TotalTaxes = result.TotalTaxes,
            Description = result.Description,
            FilePath = result.FilePath,
            ConsortiumId = result.ConsortiumId,
            Items = result.Items.Select(item => new InvoiceItemDto
            {
                Description = item.Description,
                Amount = item.Amount,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        });
        return Ok(response);
    }

}
