using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetMonthlyExpenseTotal
    {
        private readonly IExpenseRepository _repository;

        public GetMonthlyExpenseTotal(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<double> ExecuteAsync(int consortiumId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var expenses = await _repository.GetExpensesByDateRange(consortiumId, monthStart, monthEnd);

            return expenses.Sum(e => e.TotalAmount);
        }
    }
}
