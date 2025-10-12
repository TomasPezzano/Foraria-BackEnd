using ForariaDomain;

namespace Foraria.Domain.Repository
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetExpensesByDateRange(int consortiumId, DateTime startDate, DateTime endDate);
    }
}
