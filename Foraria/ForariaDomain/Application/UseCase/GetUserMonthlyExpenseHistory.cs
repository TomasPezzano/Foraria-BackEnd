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
            var now = DateTime.Now;
            var targetYear = year ?? now.Year;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = startOfYear.AddYears(1);

            var expenseDetails = await _repository.GetUserExpenses(userId);

            var lastExpensesByMonth = expenseDetails
                .SelectMany(e => e.Expenses)
                .GroupBy(exp => new { exp.CreatedAt.Year, exp.CreatedAt.Month })
                .Select(g => g.OrderByDescending(exp => exp.CreatedAt).First())
                .OrderBy(exp => exp.CreatedAt)
                .ToList();

            var paidExpenses = expenseDetails
                .Where(detail =>
                            detail.State == "Paid" &&
                            detail.Expenses.Any(exp =>
                            lastExpensesByMonth.Contains(exp) &&
                            exp.CreatedAt >= startOfYear &&
                            exp.CreatedAt < endOfYear))
                .ToList();

            var culture = new CultureInfo("es-AR");

            var monthlyGroups = paidExpenses
                .Select(detail => new
                {
                    Expense = detail.Expenses
                                .First(exp => lastExpensesByMonth.Contains(exp)) 
                })
                .GroupBy(x => x.Expense.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalPaid = paidExpenses
                        .Where(d => d.Expenses.Any(exp =>
                        lastExpensesByMonth.Contains(exp) &&
                        exp.CreatedAt.Month == g.Key))
                        .Sum(d => d.TotalAmount)
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
