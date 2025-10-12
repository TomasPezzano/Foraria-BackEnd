using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetExpensesByDateRange(int consortiumId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Expense>> GetPendingExpenses(int consortiumId);
        Task<IEnumerable<Expense>> GetUserExpensesByState(int userId, string state);
        Task<IEnumerable<Expense>> GetUserExpenses(int userId);



    }
}
