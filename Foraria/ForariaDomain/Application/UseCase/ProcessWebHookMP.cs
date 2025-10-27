﻿using System.Text.Json;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using ForariaDomain.Services;

namespace ForariaDomain.Application.UseCase
{
    public class ProcessWebHookMP
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IPaymentGateway _paymentGateway;

        public ProcessWebHookMP(
            IPaymentRepository paymentRepository,
            IExpenseRepository expenseRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentGateway paymentGateway)
        {
            _paymentRepository = paymentRepository;
            _expenseRepository = expenseRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task ExecuteAsync(JsonElement body)
        {
            string? paymentId = null;

            if (body.TryGetProperty("data", out var dataNode) &&
                dataNode.TryGetProperty("id", out var idProp))
            {
                paymentId = idProp.GetString();
            }

            if (string.IsNullOrEmpty(paymentId))
            {
                Console.WriteLine("⚠️ Webhook recibido sin data.id válido.");
                return;
            }

            Console.WriteLine($"🔔 Webhook recibido con Payment ID: {paymentId}");

            var mpPayment = await _paymentGateway.GetPaymentAsync(long.Parse(paymentId));

            if (mpPayment.Order?.Id != null)
            {
                bool isPaid = await _paymentGateway.VerifyMerchantOrderAsync(mpPayment.Order.Id.Value);

                if (isPaid)
                {
                    mpPayment.Status = "approved";
                    Console.WriteLine("✅ MerchantOrder confirma que el pago está APROBADO.");
                }
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
                catch(Exception e)
                {
                    Console.WriteLine($"⚠️ Error al parsear metadata JSON: {ex.Message}");
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
            existing.Date = mpPayment.DateApproved ?? mpPayment.DateCreated ?? DateTime.UtcNow;
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
                var expense = await _expenseRepository.GetByIdAsync(existing.ExpenseId);
                if (expense != null && expense.State != "paid")
                {
                    expense.State = "paid";
                    await _expenseRepository.SaveChangesAsync();
                    Console.WriteLine($"🏠 Expensa {expense.Id} marcada como pagada.");
                }
            }
        }
    }
}
