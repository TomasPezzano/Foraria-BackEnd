using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IExpenseRepository
    {
        /*
        Task<IEnumerable<Expense>> GetExpensesByDateRange(int consortiumId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Expense>> GetPendingExpenses(int consortiumId);
        Task<IEnumerable<Expense>> GetUserExpensesByState(int userId, string state);
        Task<IEnumerable<Expense>> GetUserExpenses(int userId);
        Task<(int totalCount, int paidCount, double totalPaidAmount, double totalUnpaidAmount)> GetMonthlyCollectionStatsAsync(int consortiumId, DateTime monthStart, DateTime monthEnd);
        */
        Task<Expense> AddExpenseAsync(Expense newExpense);
        Task<IEnumerable<Expense>> GetAllExpenses();

        Task<Expense?> GetExpenseByConsortiumAndMonthAsync(int consortiumId, string month);
        Task<Expense?> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
