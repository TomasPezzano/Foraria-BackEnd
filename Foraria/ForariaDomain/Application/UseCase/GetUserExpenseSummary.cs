using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetUserExpenseSummary
    {
        private readonly IExpenseRepository _repository;

        public GetUserExpenseSummary(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var yearStart = new DateTime(now.Year, 1, 1);

            var userExpenses = await _repository.GetUserExpenses(userId);

            //total pendiente
            var pendingExpenses = userExpenses
                .Where(e => e.State == "Pending")
                .ToList();

            var totalPending = pendingExpenses.Sum(e => e.TotalAmount);

            //facturas vencidas
            var overdueInvoices = pendingExpenses
                .Count(e => e.ExpirationDate < now);

            //total pagado este año
            var totalPaidThisYear = userExpenses
                .Where(e => e.State == "Paid" && e.CreatedAt >= yearStart)
                .Sum(e => e.TotalAmount);

            return new
            {
                totalPending = Math.Round(totalPending, 2),
                overdueInvoices,
                totalPaidThisYear = Math.Round(totalPaidThisYear, 2)
            };
        }
    }
}
