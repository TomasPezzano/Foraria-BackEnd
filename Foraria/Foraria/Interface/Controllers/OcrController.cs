using Foraria.Contracts.DTOs;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly IProcessInvoiceOcr _processInvoiceOcr;
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp" };

    public OcrController(IProcessInvoiceOcr processInvoiceOcr)
    {
        _processInvoiceOcr = processInvoiceOcr;
    }


    [HttpPost("process-invoice")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    public async Task<IActionResult> ProcessInvoice([FromForm] ProcessInvoiceRequestDto request)
    {
        // Validaciones
        if (request?.File == null || request.File.Length == 0)
        {
            return BadRequest(new ProcessInvoiceResponseDto
            {
                Success = false,
                ErrorMessage = "No se proporcionó ningún archivo"
            });
        }

        if (request.File.Length > MaxFileSizeInBytes)
        {
            return BadRequest(new ProcessInvoiceResponseDto
            {
                Success = false,
                ErrorMessage = $"El archivo excede el tamaño máximo de {MaxFileSizeInBytes / (1024.0 * 1024.0):F2} MB"
            });
        }

        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new ProcessInvoiceResponseDto
            {
                Success = false,
                ErrorMessage = $"Formato no soportado. Use: {string.Join(", ", AllowedExtensions)}"
            });
        }

        var ocrResult = await _processInvoiceOcr.ExecuteAsync(request.File);

        if (!ocrResult.Success)
        {
            return BadRequest(new ProcessInvoiceResponseDto
            {
                Success = false,
                ErrorMessage = ocrResult.ErrorMessage
            });
        }

        var response = new ProcessInvoiceResponseDto
        {
            Success = true,

            SupplierName = ocrResult.SupplierName,
            Cuit = ocrResult.Cuit,
            InvoiceDate = ocrResult.InvoiceDate,
            DueDate = ocrResult.DueDate,
            InvoiceNumber = ocrResult.InvoiceNumber,
            SubTotal = ocrResult.SubTotal,
            TotalAmount = ocrResult.TotalAmount,

            TotalTax = ocrResult.TotalTax,
            SupplierAddress = ocrResult.SupplierAddress,
            PurchaseOrder = ocrResult.PurchaseOrder,
            Description = ocrResult.Description,

            Items = ocrResult.Items.Select(item => new InvoiceItemDto
            {
                Description = item.Description,
                Amount = item.Amount,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList(),
            ConfidenceScore = ocrResult.ConfidenceScore,
            FilePath = ocrResult.FilePath,
            ProcessedAt = ocrResult.ProcessedAt
        };

        return Ok(response);
    }
}
