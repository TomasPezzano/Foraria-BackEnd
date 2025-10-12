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
        public async Task<IEnumerable<Expense>> GetPendingExpenses(int consortiumId)
        {
            return await _context.Expenses
                .Where(e => e.ConsortiumId == consortiumId &&
                            e.State == "Pending" &&
                            e.ExpirationDate >= DateTime.UtcNow)
                .OrderBy(e => e.ExpirationDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Expense>> GetUserExpensesByState(int userId, string state)
        {
            return await _context.Expenses
                .Include(e => e.Residence)
                    .ThenInclude(r => r.Users)
                .Where(e =>
                    e.Residence.Users.Any(u => u.Id == userId) &&
                    e.State == state)
                .ToListAsync();
        }
        public async Task<IEnumerable<Expense>> GetUserExpenses(int userId)
        {
            return await _context.Expenses
                .Include(e => e.Residence)
                    .ThenInclude(r => r.Users)
                .Where(e => e.Residence.Users.Any(u => u.Id == userId))
                .ToListAsync();
        }
    }
 }
