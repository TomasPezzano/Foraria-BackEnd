using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task SaveChangesAsync();
        Task<Payment?> FindByMercadoPagoMetadataAsync(Dictionary<string, object>? metadata, string? orderId, string? paymentId);

    }
}
