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
    private readonly ILocalFileStorageService _fileStorageService;

    public ProcessInvoiceOcr(IOcrService ocrService, ILogger<ProcessInvoiceOcr> logger, ILocalFileStorageService fileStorageService)
    {
        _ocrService = ocrService;
        _logger = logger;
        _fileStorageService = fileStorageService;
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

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Error en OCR para {FileName}: {Error}",
                    file.FileName,
                    result.ErrorMessage
                );
                return result;
            }

            using var stream = file.OpenReadStream();
            result.FilePath = await _fileStorageService.SaveInvoiceFileAsync(stream, file.FileName);

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Factura procesada exitosamente en {ProcessingTime}ms. " +
                "Archivo guardado en: {FilePath}, " +
                "Proveedor: {SupplierName}, CUIT: {Cuit}, " +
                "Nº Factura: {InvoiceNumber}, Total: {TotalAmount}",
                processingTime,
                result.FilePath,
                result.SupplierName ?? "N/A",
                result.Cuit ?? "N/A",
                result.InvoiceNumber ?? "N/A",
                result.TotalAmount?.ToString("C") ?? "N/A"
            );

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

            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = $"Error interno al procesar: {ex.Message}"
            };
        }
    }
}
