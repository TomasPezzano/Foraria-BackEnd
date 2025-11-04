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
            var now = DateTime.UtcNow;
            var yearStart = new DateTime(now.Year, 1, 1);

            // Trae todos los ExpenseDetails del usuario, con Expense incluido
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

            // 🟡 Total pendiente: suma de los ExpenseDetails con estado "Pending"
            var pendingExpenses = userExpenses
                .Where(e => e.State == "Pending")
                .ToList();

            var totalPending = pendingExpenses.Sum(e => e.TotalAmount);

            // 🔴 Facturas vencidas: expensas pendientes cuya expensa general está vencida
            var overdueInvoices = pendingExpenses
                .Count(e => e.Expense != null && e.Expense.ExpirationDate < now);

            // 🟢 Total pagado este año: suma de expensas pagadas con Expense del año actual
            var totalPaidThisYear = userExpenses
                .Where(e => e.State == "Paid" && e.Expense.CreatedAt >= yearStart)
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
