using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IExpenseRepository
    {
        
        Task<IEnumerable<Expense>> GetExpensesByDateRange(DateTime startDate, DateTime endDate);      
        Task<IEnumerable<Expense>> GetPendingExpenses(int consortiumId);
        Task<(int totalCount, int paidCount, double totalPaidAmount, double totalUnpaidAmount)> GetMonthlyCollectionStatsAsync(int consortiumId, DateTime monthStart, DateTime monthEnd);
        Task<Expense> AddExpenseAsync(Expense newExpense);
        Task<IEnumerable<Expense>> GetAllExpenses();
        Task<Expense?> GetExpenseByConsortiumAndMonthAsync(string month);
        Task<Expense?> GetByIdAsync(int id);
        Task SaveChangesAsync();
        Task<IEnumerable<Expense>> GetExpensesExpiringBetweenAsync(DateTime startDate, DateTime endDate);

    }
}
