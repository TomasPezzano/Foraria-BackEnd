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

        public async Task<Expense> AddExpenseAsync(Expense newExpense)
        {
            _context.Expenses.Add(newExpense);
            await _context.SaveChangesAsync();
            return newExpense;
        }

        public async Task<IEnumerable<Expense>> GetAllExpenses()
        {
            return await _context.Expenses.Include(e => e.Invoices).ToListAsync();
        }

        public async Task<Expense?> GetExpenseByConsortiumAndMonthAsync(int consortiumId, string month)
        {
            var partes = month.Split('-');
            int anio = int.Parse(partes[0]);
            int mesNumero = int.Parse(partes[1]);

            return await _context.Expenses
                .Where(e => e.ConsortiumId == consortiumId &&
                            e.CreatedAt.Year == anio &&
                            e.CreatedAt.Month == mesNumero)
                .FirstOrDefaultAsync();
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
                .Include(e => e.ExpenseDetailsByResidence)
                .Where(e => e.ConsortiumId == consortiumId &&
                            e.ExpenseDetailsByResidence.Any(d => d.State == "Pending") &&
                            e.ExpirationDate >= DateTime.UtcNow)
                .OrderBy(e => e.ExpirationDate)
                .ToListAsync();
        }

        public async Task<(int totalCount, int paidCount, double totalPaidAmount, double totalUnpaidAmount)> GetMonthlyCollectionStatsAsync(int consortiumId, DateTime monthStart, DateTime monthEnd)
        {
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseDetailsByResidence)
                .Where(e => e.ConsortiumId == consortiumId &&
                            e.CreatedAt >= monthStart &&
                            e.CreatedAt < monthEnd)
                .ToListAsync();


            var allDetails = expenses.SelectMany(e => e.ExpenseDetailsByResidence).ToList();

            var totalCount = allDetails.Count;
            var paidDetails = allDetails.Where(d => d.State == "Paid").ToList();
            var unpaidDetails = allDetails.Where(d => d.State == "Pending").ToList();

            var paidCount = paidDetails.Count;
            var totalPaidAmount = paidDetails.Sum(d => d.TotalAmount);
            var totalUnpaidAmount = unpaidDetails.Sum(d => d.TotalAmount);

            return (totalCount, paidCount, totalPaidAmount, totalUnpaidAmount);
        }




        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
