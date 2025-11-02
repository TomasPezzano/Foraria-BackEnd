using ForariaDomain.Repository;
using ForariaDomain;
using Foraria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ForariaContext _context;

        public PaymentRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Payment?> FindByMercadoPagoMetadataAsync(Dictionary<string, object>? metadata, string? orderId, string? paymentId)
        {
            int expenseId = 0, residenceId = 0;

            if (metadata != null)
            {
                if (metadata.TryGetValue("expense_id", out var expenseObj))
                    expenseId = Convert.ToInt32(expenseObj);
                if (metadata.TryGetValue("residence_id", out var residenceObj))
                    residenceId = Convert.ToInt32(residenceObj);
            }

            var query = _context.Payments.AsQueryable();

            if (expenseId > 0 && residenceId > 0)
            {
                var found = await query.FirstOrDefaultAsync(p =>
                    p.ExpenseId == expenseId &&
                    p.ResidenceId == residenceId &&
                    p.MercadoPagoPaymentId == null);
                if (found != null) return found;
            }

            if (!string.IsNullOrEmpty(orderId))
            {
                var found = await query.FirstOrDefaultAsync(p => p.PreferenceId == orderId);
                if (found != null) return found;
            }

            if (!string.IsNullOrEmpty(paymentId))
            {
                var found = await query.FirstOrDefaultAsync(p => p.MercadoPagoPaymentId == paymentId);
                if (found != null) return found;
            }

            return null;
        }

    }
}
