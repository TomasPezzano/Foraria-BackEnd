using Foraria.Infrastructure.Persistence;
using Foraria.Migrations;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foraria.Infrastructure.Infrastructure.Persistence;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ForariaContext _context;

    public InvoiceRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoice(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoices()
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesByMonthAndConsortium(DateTime inicio)
    {
        var fin = inicio.AddMonths(1).AddDays(-1);

        return await _context.Invoices.Where(i => i.CreatedAt >= inicio && i.CreatedAt <= fin && i.ResidenceId == null).Include(i => i.Items).ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetExtraordinaryInvoicesByResidenceIdAsync(int residenceId)
    {
        var invoices = await _context.Invoices
           .Include(i => i.Items)
           .Include(i => i.Expenses)
           .Where(i => i.ResidenceId == residenceId)
           .OrderByDescending(i => i.CreatedAt)
           .ToListAsync();

        return invoices;
    }

    public Task UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }
}
