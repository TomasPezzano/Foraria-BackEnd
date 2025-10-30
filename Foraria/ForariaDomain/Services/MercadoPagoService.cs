using Foraria.Contracts.DTOs;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MP = MercadoPago.Resource.Payment;

namespace ForariaDomain.Services
{
    public interface IPaymentGateway
    {
        Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(decimal amount, int expenseId, int residenceId);
        Task<MercadoPagoPaymentDto> GetPaymentAsync(long id);
        Task<bool> VerifyMerchantOrderAsync(long orderId);

    }
    public class MercadoPagoService : IPaymentGateway
    {
        private readonly PaymentClient _paymentClient;
        private readonly MerchantOrderClient _orderClient;

        public MercadoPagoService()
        {
            _paymentClient = new PaymentClient();
            _orderClient = new MerchantOrderClient();
        }

        public async Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(decimal amount, int expenseId, int residenceId)
        {
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = "Expensa mensual",
                    Quantity = 1,
                    CurrencyId = "ARS",
                    UnitPrice = amount
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
            Preference preference = await client.CreateAsync(request);

            return (preference.Id, preference.InitPoint);
        }


        public async Task<MercadoPagoPaymentDto> GetPaymentAsync(long paymentId)
        {
            var payment = await _paymentClient.GetAsync(paymentId);

            return new MercadoPagoPaymentDto
            {
                Id = (long)payment.Id,
                Status = payment.Status,
                StatusDetail = payment.StatusDetail,
                TransactionAmount = payment.TransactionAmount,
                Metadata = payment.Metadata,
                Order = new MercadoPagoOrderDto { Id = payment.Order?.Id },
                PaymentMethodId = payment.PaymentMethodId,
                DateCreated = payment.DateCreated,
                DateApproved = payment.DateApproved,
                Installments = payment.Installments,
                TransactionDetails = new TransactionDetailsDto
                {
                    InstallmentAmount = payment.TransactionDetails?.InstallmentAmount
                }
            };
        }

        public async Task<bool> VerifyMerchantOrderAsync(long orderId)
        {
            var orderClient = new MerchantOrderClient();
            var order = await orderClient.GetAsync(orderId);
            return order.Payments != null && order.Payments.Any(p => p.Status == "approved");
        }
    }
}
