using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using Foraria.Migrations;
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

            var expenses =  await _context.Expenses
                            .Include(e => e.Invoices)
                            .Include(e => e.ExpenseDetailsByResidence)
                                    .ThenInclude(ed => ed.Residence)
                                        .ThenInclude(r => r.Invoices)
                            .Include(e => e.ExpenseDetailsByResidence)
                                    .ThenInclude(ed => ed.Residence)
                                        .ThenInclude(r => r.Users)
                                            .ThenInclude(u => u.Role)
                            .ToListAsync();

            foreach (var expense in expenses)
            {
                // ids de invoices de la expensa
                var invoiceIds = expense.Invoices.Select(x => x.Id).ToHashSet();

                // filtro las invoices de cada residence
                foreach (var detail in expense.ExpenseDetailsByResidence)
                {
                    detail.Residence.Invoices = detail.Residence.Invoices
                        .Where(inv => !invoiceIds.Contains(inv.Id))
                        .ToList();
                }
            }

            return expenses;
        }

        public async Task<Expense?> GetExpenseByConsortiumAndMonthAsync(string month)
        {
            var partes = month.Split('-');
            int anio = int.Parse(partes[0]);
            int mesNumero = int.Parse(partes[1]);

            return await _context.Expenses
                    .Where(e => e.CreatedAt.Year == anio &&
                            e.CreatedAt.Month == mesNumero)
                    .OrderByDescending(e => e.Id)   
                    .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
            .Where(e =>  e.CreatedAt >= startDate &&
                        e.CreatedAt < endDate)
            .ToListAsync();
        }
        public async Task<IEnumerable<Expense>> GetPendingExpenses()
        {
            return await _context.Expenses
                .Include(e => e.ExpenseDetailsByResidence)
                .Where(e => e.ExpenseDetailsByResidence.Any(d => d.State == "Pending") &&
                            e.ExpirationDate >= DateTime.Now)
                .OrderBy(e => e.ExpirationDate)
                .ToListAsync();
        }

        public async Task<(int totalCount, int paidCount, double totalPaidAmount, double totalUnpaidAmount)> GetMonthlyCollectionStatsAsync(DateTime monthStart, DateTime monthEnd)
        {
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseDetailsByResidence)
                .Where(e => e.CreatedAt >= monthStart &&
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

        public async Task<IEnumerable<Expense>> GetExpensesExpiringBetweenAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Include(e => e.Consortium)
                .Where(e => e.ExpirationDate >= startDate && e.ExpirationDate < endDate)
                .OrderBy(e => e.ExpirationDate)
                .ToListAsync();
        }


        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Expense?> GetExpensesByUserAndDateRangeAsync(int year, int month)
        {
            return await _context.Expenses
            .Include(e => e.Invoices)
            .Where(e => e.CreatedAt.Year == year &&
                        e.CreatedAt.Month == month)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();
        }
    }
}
