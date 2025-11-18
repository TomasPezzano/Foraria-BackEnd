using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetUserExpenseSummary
    {
        private readonly IExpenseDetailRepository _repository;

        public GetUserExpenseSummary(IExpenseDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(int userId)
        {
            var now = DateTime.Now;

            var userExpenses = await _repository.GetUserExpenses(userId);

            if (userExpenses == null || !userExpenses.Any())
            {
                return new
                {
                    totalPending = 0.0,
                    overdueInvoices = 0,
                    totalPaidThisYear = 0.0
                };
            }

            var pendingExpenses = userExpenses
                .Where(e => e.State == "Pending")
                .ToList();

            var totalPending = pendingExpenses.Sum(e => e.TotalAmount);

            var lastExpensesByMonth = userExpenses
                .SelectMany(e => e.Expenses)               
                .GroupBy(exp => new { exp.CreatedAt.Year, exp.CreatedAt.Month }) 
                .Select(g => g.OrderByDescending(exp => exp.CreatedAt).First()) 
                .OrderBy(exp => exp.CreatedAt)           
                .ToList();


            var overdueInvoices = userExpenses
                .Where(detail =>
                    detail.State == "Pending" &&
                    detail.Expenses.Any(exp =>
                    lastExpensesByMonth.Contains(exp) &&
                    exp.ExpirationDate < now))
                .Count();


            var totalPaidThisYear = userExpenses
                .Where(detail =>
                    detail.State == "Paid" &&
                    detail.Expenses.Any(exp =>
                    lastExpensesByMonth.Contains(exp) &&
                    exp.CreatedAt.Year == now.Year)) 
                .Sum(detail => detail.TotalAmount);

            return new
            {
                totalPending = Math.Round(totalPending, 2),
                overdueInvoices,
                totalPaidThisYear = Math.Round(totalPaidThisYear, 2)
            };
        }
    }
}
