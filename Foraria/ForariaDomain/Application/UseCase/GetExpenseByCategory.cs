using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetExpenseByCategory
    {
        private readonly IExpenseRepository _repository;

        public GetExpenseByCategory(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<object>> ExecuteAsync(int consortiumId, DateTime? date = null)
        {
            var now = date ?? DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var expenses = await _repository.GetExpensesByDateRange(consortiumId, monthStart, monthEnd);

            var grouped = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.TotalAmount)
                })
                .ToList();

            var grandTotal = grouped.Sum(g => g.Total);

            return grouped
                .Select(g => new
                {
                    category = g.Category,
                    total = Math.Round(g.Total, 2),
                    percentage = grandTotal == 0 ? 0 : Math.Round((g.Total / grandTotal) * 100, 2)
                })
                .OrderByDescending(x => x.percentage)
                .ToList();
        }
    }
}