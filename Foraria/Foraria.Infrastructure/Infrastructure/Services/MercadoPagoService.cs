using ForariaDomain.Models;
using MercadoPago.Client.MerchantOrder;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MP = MercadoPago.Resource.Payment;


namespace ForariaDomain.Services
{
    public class MercadoPagoService : IPaymentService
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
                    Success = "https://foraria.vercel.app/expensas",
                    Failure = "https://foraria.vercel.app/expensas",
                    Pending = "https://foraria.vercel.app/expensas"
                },
                AutoReturn = "approved",
                NotificationUrl = "https://foraria.vercel.app/webhook",
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


        public async Task<MercadoPagoPayment> GetPaymentAsync(long paymentId)
        {
            var payment = await _paymentClient.GetAsync(paymentId);

            return new MercadoPagoPayment
            {
                Id = (long)payment.Id,
                Status = payment.Status,
                StatusDetail = payment.StatusDetail,
                TransactionAmount = payment.TransactionAmount,
                Metadata = payment.Metadata,
                Order = new MercadoPagoOrder { Id = payment.Order?.Id },
                PaymentMethodId = payment.PaymentMethodId,
                DateCreated = payment.DateCreated,
                DateApproved = payment.DateApproved,
                Installments = payment.Installments,
                TransactionDetails = new TransactionDetails
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
