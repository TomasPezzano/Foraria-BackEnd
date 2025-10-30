using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository
{
    public interface IPaymentMethodRepository
    {
        Task AddAsync(Payment payment);
        Task<PaymentMethod> GetOrCreateAsync(string mpPaymentMethodId);
        Task UpdateAsync(Payment payment);
        Task<Payment?> GetByPreferenceIdAsync(string preferenceId);
        Task SaveChangesAsync();

    }
}
