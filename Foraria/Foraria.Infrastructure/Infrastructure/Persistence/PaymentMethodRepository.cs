using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Infrastructure.Persistence
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ForariaContext _context;

        public PaymentMethodRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<PaymentMethod> GetOrCreateAsync(string paymentMethodName)
        {
            var existing = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Name == paymentMethodName);

            if (existing != null)
                return existing;

            var newMethod = new PaymentMethod
            {
                Name = paymentMethodName
            };

            _context.PaymentMethods.Add(newMethod);
            await _context.SaveChangesAsync();

            return newMethod;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }

        public async Task<Payment?> GetByPreferenceIdAsync(string preferenceId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.PreferenceId == preferenceId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
