using System.Text.Json;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using MercadoPago.Resource.Payment;

namespace ForariaDomain.Application.UseCase;

public class ProcessWebHookMP
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IPaymentService _paymentGateway;
    private readonly IExpenseDetailRepository _expenseDetailRepository;

    public ProcessWebHookMP(
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IPaymentService paymentGateway,
        IExpenseDetailRepository expenseDetailRepository)
    {
        _paymentRepository = paymentRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _paymentGateway = paymentGateway;
        _expenseDetailRepository = expenseDetailRepository;
    }

    public async Task ExecuteAsync(JsonElement body, bool isMerchantOrder = false)
    {
        string? paymentId = null;
        string? orderId = null;

        if (body.TryGetProperty("data", out var dataNode))
        {
            if (dataNode.TryGetProperty("id", out var idProp))
            {
                paymentId = idProp.GetString();  
            }
            if (dataNode.TryGetProperty("order_id", out var orderIdProp))
            {
                orderId = orderIdProp.GetString();  
            }
        }

        if (isMerchantOrder && !string.IsNullOrEmpty(orderId))
        {
            Console.WriteLine($"🔔 Procesando Merchant Order con ID: {orderId}");

            bool isPaid = await _paymentGateway.VerifyMerchantOrderAsync(long.Parse(orderId));

            if (isPaid)
            {
                Console.WriteLine("✅ Merchant Order aprobado.");
            }
            else
            {
                Console.WriteLine("⚠️ Merchant Order no está aprobado.");
            }
        }
        else if (!string.IsNullOrEmpty(paymentId))
        {
            Console.WriteLine($"🔔 Procesando Payment con ID: {paymentId}");

            var mpPayment = await _paymentGateway.GetPaymentAsync(long.Parse(paymentId));

            if (mpPayment.Status == "approved")
            {
                Console.WriteLine("✅ Pago aprobado.");
            }
            else
            {
                Console.WriteLine("⚠️ Pago no aprobado.");
            }

            Dictionary<string, object>? metadataDict = null;
            object? metadataObj = mpPayment.Metadata;

            if (metadataObj is IDictionary<string, object> dict)
            {
                metadataDict = dict.ToDictionary(k => k.Key, v => v.Value);
            }
            else if (metadataObj is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                metadataDict = je.EnumerateObject()
                                 .ToDictionary(p => p.Name, p => (object)p.Value.ToString());
            }
            else if (metadataObj is string jsonString && !string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        metadataDict = root.EnumerateObject()
                                           .ToDictionary(p => p.Name, p => (object)p.Value.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"⚠️ Error al parsear metadata JSON: {e.Message}");
                    metadataDict = null;
                }
            }
            else
            {
                Console.WriteLine("⚠️ Metadata ausente o en formato desconocido.");
            }

            var existing = await _paymentRepository.FindByMercadoPagoMetadataAsync(
                metadataDict, mpPayment.Order?.Id?.ToString(), paymentId
            );

            if (existing == null)
            {
                Console.WriteLine("❌ No se encontró Payment relacionado. Ignorando webhook.");
                return;
            }

            if (existing.Status == mpPayment.Status)
            {
                Console.WriteLine($"⚙️ Webhook duplicado ignorado. Estado '{mpPayment.Status}' ya procesado.");
                return;
            }

            existing.MercadoPagoPaymentId = mpPayment.Id.ToString();
            existing.Status = mpPayment.Status;
            existing.StatusDetail = mpPayment.StatusDetail;
            existing.Amount = mpPayment.TransactionAmount ?? existing.Amount;
            existing.Date = mpPayment.DateApproved ?? mpPayment.DateCreated ?? DateTime.Now;
            existing.Installments = mpPayment.Installments;
            existing.InstallmentAmount = mpPayment.TransactionDetails?.InstallmentAmount;

            if (!string.IsNullOrEmpty(mpPayment.PaymentMethodId))
            {
                var method = await _paymentMethodRepository.GetOrCreateAsync(mpPayment.PaymentMethodId);
                existing.PaymentMethodId = method.Id;
            }

            await _paymentRepository.SaveChangesAsync();
            Console.WriteLine($"✅ Pago actualizado. Estado: {existing.Status}");

            if (mpPayment.Status == "approved")
            {
                var expense = await _expenseDetailRepository.GetExpenseDetailById(existing.ExpenseDetailByResidenceId);
                if (expense != null && expense.State != "paid")
                {
                    expense.State = "paid";
                    await _expenseDetailRepository.SaveChangesAsync();
                    Console.WriteLine($"🏠 Expensa {expense.Id} marcada como pagada.");
                }
            }
        }
        else
        {
            Console.WriteLine("⚠️ Webhook recibido sin un ID válido (paymentId u orderId).");
        }
    }


}
