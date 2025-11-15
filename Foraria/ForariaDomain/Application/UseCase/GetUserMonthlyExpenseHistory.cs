using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using System.Globalization;

namespace Foraria.Application.UseCase
{
    public class GetUserMonthlyExpenseHistory
    {
        private readonly IExpenseDetailRepository _repository;

        public GetUserMonthlyExpenseHistory(IExpenseDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(int userId, int? year = null)
        {
            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = startOfYear.AddYears(1);

            var expenseDetails = await _repository.GetUserExpenses(userId);

            var paidExpenses = expenseDetails
                .Where(e => e.State == "Paid"
                            && e.Expense.CreatedAt >= startOfYear
                            && e.Expense.CreatedAt < endOfYear)
                .ToList();

            var culture = new CultureInfo("es-AR");

            var monthlyGroups = paidExpenses
                .GroupBy(e => e.Expense.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalPaid = g.Sum(e => e.TotalAmount),

                })
                .ToDictionary(x => x.Month, x => x);

            var monthlyHistory = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    month = culture.DateTimeFormat.GetMonthName(m),
                    totalPaid = Math.Round(monthlyGroups.ContainsKey(m) ? monthlyGroups[m].TotalPaid : 0, 2)
                  
                })
                .ToList();

            return new
            {
                userId,
                year = targetYear,
                monthlyHistory
            };
        }
    }

}
