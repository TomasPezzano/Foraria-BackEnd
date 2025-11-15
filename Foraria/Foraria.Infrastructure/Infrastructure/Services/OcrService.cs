using Azure;
using Azure.AI.DocumentIntelligence;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Foraria.Infrastructure.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly ILogger<OcrService> _logger;
    private const string PrebuiltInvoiceModel = "prebuilt-invoice";

    public OcrService(IConfiguration configuration, ILogger<OcrService> logger)
    {
        _logger = logger;
        
        var endpoint = configuration["AzureDocumentIntelligence:Endpoint"];
        var apiKey = configuration["AzureDocumentIntelligence:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "Las credenciales de Azure Document Intelligence no fueron configuradas. " +
                "Por favor agregue 'AzureDocumentIntelligence:Endpoint' y 'AzureDocumentIntelligence:ApiKey' en appsettings.json"
            );
        }

        _client = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );
    }

    public async Task<InvoiceOcrResult> ProcessInvoiceAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new InvoiceOcrResult
                {
                    Success = false,
                    ErrorMessage = "No se proporcionó ningún archivo"
                };
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return new InvoiceOcrResult
                {
                    Success = false,
                    ErrorMessage = $"Formato no soportado. Use: {string.Join(", ", allowedExtensions)}"
                };
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var fileData = BinaryData.FromStream(memoryStream);

            _logger.LogInformation("Iniciando análisis OCR para archivo: {FileName}", file.FileName);

            Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                PrebuiltInvoiceModel,
                fileData
            );

            var result = operation.Value;

            if (result.Documents == null || result.Documents.Count == 0)
            {
                return new InvoiceOcrResult
                {
                    Success = false,
                    ErrorMessage = "No se pudo detectar una factura en el documento"
                };
            }

            var invoice = result.Documents[0];

            var totalAmount = GetFieldValueAsDecimal(invoice.Fields, "InvoiceTotal")
                             ?? GetFieldValueAsDecimal(invoice.Fields, "AmountDue");
            var subTotal = GetFieldValueAsDecimal(invoice.Fields, "SubTotal");
            var totalTax = GetFieldValueAsDecimal(invoice.Fields, "TotalTax");

            if (totalAmount.HasValue && subTotal.HasValue)
            {
                var calculatedTax = totalAmount.Value - subTotal.Value;
                
                if (!totalTax.HasValue || Math.Abs(totalTax.Value - calculatedTax) > 1)
                {
                    _logger.LogWarning(
                        "TotalTax recalculado: {Calculated} (OCR retornó: {Original})",
                        calculatedTax,
                        totalTax ?? 0
                    );
                    totalTax = calculatedTax;
                }
            }

            var ocrResult = new InvoiceOcrResult
            {
                Success = true,

                SupplierName = GetFieldValue(invoice.Fields, "VendorName"),
                Cuit = ExtractCuit(invoice.Fields, result.Content),
                InvoiceDate = GetFieldValueAsDateTime(invoice.Fields, "InvoiceDate"),
                DueDate = GetFieldValueAsDateTime(invoice.Fields, "DueDate"),
                InvoiceNumber = GetFieldValue(invoice.Fields, "InvoiceId"),
                SubTotal = subTotal,
                TotalAmount = totalAmount,
                TotalTax = totalTax,

                SupplierAddress = GetFieldValue(invoice.Fields, "VendorAddress"),
                PurchaseOrder = GetFieldValue(invoice.Fields, "PurchaseOrder"),
                Description = GetFieldValue(invoice.Fields, "PurchaseOrder")
                             ?? GetFieldValue(invoice.Fields, "InvoiceId")
                             ?? "Factura procesada",

                Items = ExtractItems(invoice.Fields),
                ConfidenceScore = invoice.Confidence,
                ProcessedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(ocrResult.Cuit) && !IsValidCuit(ocrResult.Cuit))
            {
                _logger.LogWarning("CUIT extraído es inválido: {Cuit}", ocrResult.Cuit);
            }

            return ocrResult;
        }
        catch (RequestFailedException ex) when (ex.Status == 401)
        {
            _logger.LogError(ex, "Error de autenticación con Azure");
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = "Error de autenticación con Azure. Verifique las credenciales."
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 429)
        {
            _logger.LogError(ex, "Límite de solicitudes excedido");
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = "Límite de solicitudes excedido. Intente nuevamente más tarde."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar documento");
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = $"Error al procesar el documento: {ex.Message}"
            };
        }
    }

    private string? GetFieldValue(IReadOnlyDictionary<string, DocumentField> fields, string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var field) && !string.IsNullOrEmpty(field.Content))
        {
            return field.Content
                .Replace("\n", " ")         
                .Replace("\r", "")           
                .Replace("®", "")            
                .Replace("©", "")
                .Replace("™", "")
                .Trim()
                .Replace("  ", " ");        
        }
        return null;
    }

    private DateTime? GetFieldValueAsDateTime(IReadOnlyDictionary<string, DocumentField> fields, string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var field)
            && field.FieldType == DocumentFieldType.Date
            && field.ValueDate.HasValue)
        {
            return field.ValueDate.Value.DateTime;
        }
        return null;
    }

    private decimal? GetFieldValueAsDecimal(IReadOnlyDictionary<string, DocumentField> fields, string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var field))
        {
            if (field.FieldType == DocumentFieldType.Currency && field.ValueCurrency != null)
            {
                return (decimal)field.ValueCurrency.Amount;
            }
            else if (field.FieldType == DocumentFieldType.Double && field.ValueDouble.HasValue)
            {
                return (decimal)field.ValueDouble.Value;
            }
            else if (!string.IsNullOrEmpty(field.Content)
                     && decimal.TryParse(
                         field.Content.Replace("$", "").Replace(",", "").Trim(),
                         out decimal amount))
            {
                return amount;
            }
        }
        return null;
    }

    private string? ExtractCuit(IReadOnlyDictionary<string, DocumentField> fields, string? fullContent)
    {
        var taxId = GetFieldValue(fields, "VendorTaxId");
        if (!string.IsNullOrEmpty(taxId))
        {
            var cleaned = CleanCuit(taxId);
            if (!string.IsNullOrEmpty(cleaned))
                return cleaned;
        }

        if (!string.IsNullOrEmpty(fullContent))
        {
            var cuitPattern = @"\b\d{2}-\d{8}-\d{1}\b|\b\d{11}\b";
            var match = System.Text.RegularExpressions.Regex.Match(fullContent, cuitPattern);

            if (match.Success)
            {
                return CleanCuit(match.Value);
            }
        }

        return null;
    }

    private string? CleanCuit(string cuit)
    {
        if (string.IsNullOrEmpty(cuit))
            return null;

        var cleaned = System.Text.RegularExpressions.Regex.Replace(cuit, @"[^\d]", "");
        
        return cleaned.Length == 11 ? cleaned : null;
    }

    private bool IsValidCuit(string cuit)
    {
        if (string.IsNullOrEmpty(cuit) || cuit.Length != 11 || !cuit.All(char.IsDigit))
            return false;

        int[] multiplicadores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        int suma = 0;

        for (int i = 0; i < 10; i++)
        {
            suma += int.Parse(cuit[i].ToString()) * multiplicadores[i];
        }

        int verificador = 11 - (suma % 11);
        if (verificador == 11) verificador = 0;
        if (verificador == 10) verificador = 9;

        return verificador == int.Parse(cuit[10].ToString());
    }

    private List<InvoiceItem> ExtractItems(IReadOnlyDictionary<string, DocumentField> fields)
    {
        var items = new List<InvoiceItem>();

        if (fields.TryGetValue("Items", out var itemsField)
            && itemsField.FieldType == DocumentFieldType.List
            && itemsField.ValueList != null)
        {
            foreach (var item in itemsField.ValueList)
            {
                if (item.FieldType == DocumentFieldType.Dictionary && item.ValueDictionary != null)
                {
                    var itemDict = item.ValueDictionary;

                    var description = GetItemField(itemDict, "Description")
                                    ?? GetItemField(itemDict, "ProductCode")
                                    ?? "Item";

                    var amount = GetItemFieldAsDecimal(itemDict, "Amount");
                    var quantity = GetItemFieldAsInt(itemDict, "Quantity");
                    var unitPrice = GetItemFieldAsDecimal(itemDict, "UnitPrice");


                    if ((!quantity.HasValue || quantity == 0) && 
                        amount.HasValue && 
                        unitPrice.HasValue &&
                        amount.Value < 100 &&        
                        unitPrice.Value > 1000)   
                    {
                        _logger.LogWarning(
                            "Item con valores invertidos detectados: {Description}. " +
                            "Amount={Amount}, UnitPrice={UnitPrice}. Corrigiendo...",
                            description, amount, unitPrice
                        );

                        var tempAmount = amount.Value;
                        quantity = (int)tempAmount;
                        amount = unitPrice;
                        unitPrice = amount.Value / quantity.Value;
                    }

                    if (quantity.HasValue && quantity > 0)
                    {
                        if (amount.HasValue && !unitPrice.HasValue)
                        {
                            unitPrice = amount.Value / quantity.Value;
                            _logger.LogInformation(
                                "UnitPrice calculado para '{Description}': {UnitPrice}",
                                description, unitPrice
                            );
                        }
                        else if (unitPrice.HasValue && !amount.HasValue)
                        {
                            amount = unitPrice.Value * quantity.Value;
                            _logger.LogInformation(
                                "Amount calculado para '{Description}': {Amount}",
                                description, amount
                            );
                        }
                    }

                    items.Add(new InvoiceItem
                    {
                        Description = description,
                        Amount = amount,
                        Quantity = quantity ?? 1,  
                        UnitPrice = unitPrice
                    });
                }
            }
        }

        return items;
    }

    private string? GetItemField(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        if (dict.TryGetValue(key, out var field) && !string.IsNullOrEmpty(field.Content))
        {
            return field.Content
                .Replace("\n", " ")
                .Replace("\r", "")
                .Trim();
        }
        return null;
    }

    private decimal? GetItemFieldAsDecimal(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        if (dict.TryGetValue(key, out var field))
        {
            if (field.FieldType == DocumentFieldType.Currency && field.ValueCurrency != null)
            {
                return (decimal)field.ValueCurrency.Amount;
            }
            else if (field.FieldType == DocumentFieldType.Double && field.ValueDouble.HasValue)
            {
                return (decimal)field.ValueDouble.Value;
            }
            else if (field.FieldType == DocumentFieldType.Int64 && field.ValueInt64.HasValue)
            {
                return (decimal)field.ValueInt64.Value;
            }
        }
        return null;
    }

    private int? GetItemFieldAsInt(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        if (dict.TryGetValue(key, out var field))
        {
            if (field.FieldType == DocumentFieldType.Int64 && field.ValueInt64.HasValue)
            {
                return (int)field.ValueInt64.Value;
            }
            else if (field.FieldType == DocumentFieldType.Double && field.ValueDouble.HasValue)
            {
                return (int)field.ValueDouble.Value;
            }
        }
        return null;
    }
}