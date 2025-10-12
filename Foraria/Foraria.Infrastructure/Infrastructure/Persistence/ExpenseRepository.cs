using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ForariaContext _context;

        public ExpenseRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRange(int consortiumId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.ConsortiumId == consortiumId &&
                            e.CreatedAt >= startDate &&
                            e.CreatedAt < endDate)
                .ToListAsync();
        }
    }
}
