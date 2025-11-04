using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetMonthlyExpenseTotal
    {
        private readonly IInvoiceRepository _repository;

        public GetMonthlyExpenseTotal(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<double> ExecuteAsync(int consortiumId)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var invoices = await _repository.GetAllInvoicesByMonthAndConsortium(startOfMonth, consortiumId);

            var total = invoices
                .Where(i => i.DateOfIssue >= startOfMonth && i.DateOfIssue <= endOfMonth)
                .Sum(i => (double)i.Amount);

            return total;
        }
    }
}
