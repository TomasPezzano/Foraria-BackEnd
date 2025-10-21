using ForariaDomain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Application.UseCase;

public interface IProcessInvoiceOcr
{
    Task<InvoiceOcrResult> ExecuteAsync(IFormFile file);
}

public class ProcessInvoiceOcr : IProcessInvoiceOcr
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<ProcessInvoiceOcr> _logger;

    public ProcessInvoiceOcr(IOcrService ocrService, ILogger<ProcessInvoiceOcr> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task<InvoiceOcrResult> ExecuteAsync(IFormFile file)
    {
        _logger.LogInformation(
            "Iniciando procesamiento de factura: {FileName} ({FileSize} bytes)",
            file.FileName,
            file.Length
        );

        var startTime = DateTime.UtcNow;

        try
        {
            var result = await _ocrService.ProcessInvoiceAsync(file);

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (result.Success)
            {
                // ⭐ Logging mejorado con los nuevos campos
                _logger.LogInformation(
                    "Factura procesada exitosamente en {ProcessingTime}ms. " +
                    "Proveedor: {SupplierName}, CUIT: {Cuit}, " +
                    "Nº Factura: {InvoiceNumber}, Fecha: {InvoiceDate}, " +
                    "Vencimiento: {DueDate}, Subtotal: {SubTotal}, Total: {TotalAmount}, " +
                    "Confianza: {Confidence}",
                    processingTime,
                    result.SupplierName ?? "N/A",
                    result.Cuit ?? "N/A",
                    result.InvoiceNumber ?? "N/A",
                    result.InvoiceDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    result.DueDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    result.SubTotal?.ToString("C") ?? "N/A",
                    result.TotalAmount?.ToString("C") ?? "N/A",
                    result.ConfidenceScore
                );
            }
            else
            {
                _logger.LogWarning(
                    "Error al procesar factura {FileName} después de {ProcessingTime}ms: {Error}",
                    file.FileName,
                    processingTime,
                    result.ErrorMessage
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(
                ex,
                "Excepción al procesar factura {FileName} después de {ProcessingTime}ms",
                file.FileName,
                processingTime
            );
            throw;
        }
    }
}
