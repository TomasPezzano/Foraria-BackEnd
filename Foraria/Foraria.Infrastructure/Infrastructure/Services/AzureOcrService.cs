using Azure;
using Azure.AI.DocumentIntelligence;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Foraria.Infrastructure.Infrastructure.Services;

public class AzureOcrService : IOcrService
{

    private readonly DocumentIntelligenceClient _client;
    private const string PrebuiltInvoiceModel = "prebuilt-invoice";

    public AzureOcrService(IConfiguration configuration)
    {
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

            return new InvoiceOcrResult
            {
                Success = true,
                SupplierName = GetFieldValue(invoice.Fields, "VendorName"),
                Cuit = ExtractCuit(invoice.Fields, result.Content),
                InvoiceDate = GetFieldValueAsDateTime(invoice.Fields, "InvoiceDate"),
                TotalAmount = GetFieldValueAsDecimal(invoice.Fields, "InvoiceTotal")
                             ?? GetFieldValueAsDecimal(invoice.Fields, "AmountDue"),
                Description = GetFieldValue(invoice.Fields, "PurchaseOrder")
                             ?? GetFieldValue(invoice.Fields, "InvoiceId")
                             ?? "Factura procesada",
                Items = ExtractItems(invoice.Fields),
                ConfidenceScore = invoice.Confidence
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 401)
        {
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = "Error de autenticación con Azure. Verifique las credenciales."
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 429)
        {
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = "Límite de solicitudes excedido. Intente nuevamente más tarde."
            };
        }
        catch (Exception ex)
        {
            return new InvoiceOcrResult
            {
                Success = false,
                ErrorMessage = $"Error al procesar el documento: {ex.Message}"
            };
        }
    }

    private string? GetFieldValue(IReadOnlyDictionary<string, DocumentField> fields, string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var field))
        {
            return field.Content;
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
            return CleanCuit(taxId);
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

    private string CleanCuit(string cuit)
    {
        return System.Text.RegularExpressions.Regex.Replace(cuit, @"[^\d]", "");
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

                    items.Add(new InvoiceItem
                    {
                        Description = GetItemField(itemDict, "Description")
                                    ?? GetItemField(itemDict, "ProductCode")
                                    ?? "Item",
                        Amount = GetItemFieldAsDecimal(itemDict, "Amount"),
                        Quantity = GetItemFieldAsInt(itemDict, "Quantity"),
                        UnitPrice = GetItemFieldAsDecimal(itemDict, "UnitPrice")
                    });
                }
            }
        }

        return items;
    }

    private string? GetItemField(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        return dict.TryGetValue(key, out var field) ? field.Content : null;
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
        }
        return null;
    }

    private int? GetItemFieldAsInt(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        if (dict.TryGetValue(key, out var field)
            && field.FieldType == DocumentFieldType.Int64
            && field.ValueInt64.HasValue)
        {
            return (int)field.ValueInt64.Value;
        }
        return null;
    }

}
