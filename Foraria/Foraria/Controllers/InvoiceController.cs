using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    public readonly ICreateInvoice _createInvoice;
    public readonly IGetAllInvoices _getAllInvoices;
    private readonly IPermissionService _permissionService;

    public InvoiceController(
        ICreateInvoice createInvoice,
        IGetAllInvoices getAllInvoices,
        IPermissionService permissionService)
    {
        _createInvoice = createInvoice;
        _getAllInvoices = getAllInvoices;
        _permissionService = permissionService;
    }

    [HttpPost]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Crea una nueva factura.",
        Description = "Registra una nueva factura para un consorcio con los datos proporcionados, incluyendo los ítems asociados."
    )]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto invoiceDto)
    {
        await _permissionService.EnsurePermissionAsync(User, "Invoices.Create");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la factura no son válidos.");

        if (invoiceDto.ConsortiumId <= 0)
            throw new DomainValidationException("Debe especificar un ConsortiumId válido.");

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

        var result = await _createInvoice.Execute(invoice);
        if (result == null)
            throw new BusinessException("No se pudo crear la factura.");

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

    [HttpGet]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Obtiene todas las facturas registradas.",
        Description = "Devuelve la lista completa de facturas junto con sus ítems asociados."
    )]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        await _permissionService.EnsurePermissionAsync(User, "Invoices.ViewAll"); //admin y consorcio, igual que expensas, asumo que un usuario no va a ver TODAS las facturas

        var invoices = await _getAllInvoices.Execute();

        if (invoices == null)
            throw new NotFoundException("No se pudieron obtener las facturas.");

        if (!invoices.Any())
            throw new NotFoundException("No se encontraron facturas registradas.");

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
