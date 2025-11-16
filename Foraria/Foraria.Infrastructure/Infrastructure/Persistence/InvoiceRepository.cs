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

    public async Task<IEnumerable<Invoice>> GetAllInvoicesByMonthAndConsortium(DateTime inicio, int consortiumId)
    {
        var fin = inicio.AddMonths(1).AddDays(-1);

        return await _context.Invoices.Where(i => i.ProcessedAt >= inicio && i.ProcessedAt <= fin && i.ConsortiumId == consortiumId && i.ResidenceId == null).Include(i => i.Items).ToListAsync();
    }

    public Task UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }
}
