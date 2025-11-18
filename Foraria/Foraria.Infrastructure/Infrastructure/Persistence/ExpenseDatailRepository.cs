using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Repository;
using MercadoPago.Resource.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class ExpenseDatailRepository : IExpenseDetailRepository
{
    private readonly ForariaContext _context;

    public ExpenseDatailRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<ExpenseDetailByResidence> AddExpenseDetailAsync(ExpenseDetailByResidence newExpenseDetail)
    {
        _context.ExpenseDetailByResidences.Add(newExpenseDetail);
        await _context.SaveChangesAsync();
        return newExpenseDetail;
    }

    public async Task<ExpenseDetailByResidence> GetExpenseDetailById(int id)
    {
       return await _context.ExpenseDetailByResidences.FindAsync(id);
    }

    public async Task<ICollection<ExpenseDetailByResidence>> GetExpenseDetailByResidence(int id)
    {
        return await _context.ExpenseDetailByResidences.Where(ed => ed.ResidenceId == id).Include(e => e.Expenses).ThenInclude(ex => ex.Invoices).ToListAsync();
        
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExpenseDetailByResidence>> GetUserExpensesByState(int userId, string state)
    {
        return await _context.ExpenseDetailByResidences
        .Include(e => e.Residence)
         .ThenInclude(r => r.Users)
        .Where(e =>
         e.Residence.Users.Any(u => u.Id == userId) &&
         e.State == state)
        .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseDetailByResidence>> GetUserExpenses(int userId)
    {
        return await _context.ExpenseDetailByResidences
            .Include(e => e.Expenses)
                .ThenInclude(ex => ex.Invoices)
            .Include(e => e.Residence)
                .ThenInclude(r => r.Users)
            .Where(e => e.Residence.Users.Any(u => u.Id == userId))
            .ToListAsync();
    }


}
