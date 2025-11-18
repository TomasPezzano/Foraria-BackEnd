using ForariaDomain.Models;

namespace ForariaDomain.Services
{
    public interface IPaymentService
    {
        Task<(string PreferenceId, string InitPoint)> CreatePreferenceAsync(decimal amount, int expenseId, int residenceId);
        Task<MercadoPagoPayment> GetPaymentAsync(long id);
        Task<bool> VerifyMerchantOrderAsync(long orderId);
    }
}
