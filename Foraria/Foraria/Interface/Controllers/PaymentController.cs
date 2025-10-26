using ForariaDomain;
using Foraria.Infrastructure.Persistence;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MP = MercadoPago.Resource.Payment; // alias

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ForariaContext _context;

        public PaymentController(ForariaContext context)
        {
            _context = context;
        }

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference(int expenseId, int residenceId)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId);

            if (expense == null)
            {
                return NotFound("Expense no encontrada.");
            }

            var amount = expense.TotalAmount;

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = "Expensa mensual",
                        Quantity = 1,
                        CurrencyId = "ARS",
                        UnitPrice = (decimal?)amount
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://wagonless-hsiu-grippelike.ngrok-free.dev/swagger",
                    Failure = "https://www.facebook.com/",
                    Pending = "https://tuweb.com/pending"
                },
                AutoReturn = "approved",
                NotificationUrl = "https://wagonless-hsiu-grippelike.ngrok-free.dev/api/payment/webhook",
                Metadata = new Dictionary<string, object>
                    {
                       { "expense_id", expenseId },     
                        { "residence_id", residenceId }
                    }

            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(request);

            var payment = new Payment
            {
                PreferenceId = preference.Id,
                Date = DateTime.UtcNow,
                ExpenseId = expenseId,
                ResidenceId = residenceId,
                Status = "pending",
                Amount = (decimal)amount
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { preferenceId = preference.Id, initPoint = preference.InitPoint });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body)
        {
            string? paymentId = null;

            try
            {
                Console.WriteLine("🟡 Webhook recibido.");

                if (body.TryGetProperty("data", out var dataNode) &&
                    dataNode.TryGetProperty("id", out var idProp))
                {
                    paymentId = idProp.GetString();
                }

                if (string.IsNullOrEmpty(paymentId))
                {
                    Console.WriteLine("⚠️ Webhook recibido sin data.id válido.");
                    return Ok();
                }

                Console.WriteLine($"🔔 Webhook recibido con Payment ID: {paymentId}");

                var client = new PaymentClient();
                var mpPayment = await client.GetAsync(long.Parse(paymentId));

                if (mpPayment.Order?.Id != null)
                {
                    try
                    {
                        var orderClient = new MercadoPago.Client.MerchantOrder.MerchantOrderClient();
                        var mpOrder = await orderClient.GetAsync(mpPayment.Order.Id.Value);

                        bool fullyPaid = mpOrder.Payments != null && mpOrder.Payments.Any(p => p.Status == "approved");

                        if (fullyPaid)
                        {
                            mpPayment.Status = "approved";
                            Console.WriteLine("✅ MerchantOrder confirma que el pago está APROBADO.");
                        }
                        else
                        {
                            Console.WriteLine($"ℹ️ MerchantOrder aún no acreditado. Estado: {mpOrder.Payments.FirstOrDefault()?.Status}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error consultando MerchantOrder: {ex.Message}");
                    }
                }

                Payment? existing = null;
                int expenseId = 0;
                int residenceId = 0;

                if (mpPayment.Metadata != null)
                {
                    if (mpPayment.Metadata.TryGetValue("expense_id", out var expenseObj))
                        expenseId = Convert.ToInt32(expenseObj);
                    if (mpPayment.Metadata.TryGetValue("residence_id", out var residenceObj))
                        residenceId = Convert.ToInt32(residenceObj);
                }

                if (expenseId > 0 && residenceId > 0)
                {
                    existing = await _context.Payments.FirstOrDefaultAsync(p =>
                        p.ExpenseId == expenseId &&
                        p.ResidenceId == residenceId &&
                        p.MercadoPagoPaymentId == null);
                }

                if (existing == null && mpPayment.Order?.Id != null)
                {
                    var preferenceId = mpPayment.Order.Id.ToString();
                    existing = await _context.Payments.FirstOrDefaultAsync(p =>
                        p.PreferenceId == preferenceId);
                }

                if (existing == null)
                {
                    existing = await _context.Payments.FirstOrDefaultAsync(p =>
                        p.MercadoPagoPaymentId == paymentId);
                }

                if (existing == null)
                {
                    Console.WriteLine("❌ No se encontró Payment relacionado. No se realizará inserción.");
                    return Ok();
                }

                if (existing.Status == mpPayment.Status)
                {
                    Console.WriteLine($"⚙️ Webhook duplicado ignorado. Estado '{mpPayment.Status}' ya procesado para Payment ID {existing.Id}.");
                    return Ok();
                }

                existing.MercadoPagoPaymentId = mpPayment.Id.ToString();
                existing.Status = mpPayment.Status;
                existing.StatusDetail = mpPayment.StatusDetail;
                existing.Amount = mpPayment.TransactionAmount ?? existing.Amount;
                existing.Date = mpPayment.DateApproved ?? mpPayment.DateCreated ?? DateTime.UtcNow;
                existing.Installments = mpPayment.Installments;
                existing.InstallmentAmount = mpPayment.TransactionDetails?.InstallmentAmount;


                if (!string.IsNullOrEmpty(mpPayment.PaymentMethodId))
                {
                    var method = await _context.PaymentMethods
                        .FirstOrDefaultAsync(m => m.Name == mpPayment.PaymentMethodId);

                    if (method == null)
                    {
                        method = new PaymentMethod { Name = mpPayment.PaymentMethodId };
                        _context.PaymentMethods.Add(method);
                        await _context.SaveChangesAsync();
                    }

                    existing.PaymentMethodId = method.Id;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Pago actualizado en DB. Nuevo estado: {existing.Status}");

                if (mpPayment.Status == "approved")
                {
                    var expense = await _context.Expenses.FindAsync(existing.ExpenseId);
                    if (expense != null && expense.State != "paid")
                    {
                        expense.State = "paid";
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"🏠 Expensa {expense.Id} marcada como pagada.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando webhook: {ex.Message}");
            }

            
            return Ok();
        }

    }
}
