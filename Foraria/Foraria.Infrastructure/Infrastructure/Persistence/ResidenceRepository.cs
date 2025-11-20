using Azure;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Services;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ResidenceRepository : IResidenceRepository
{
    private readonly ForariaContext _context;
    private readonly ITenantContext _tenantContext;

    public ResidenceRepository(ForariaContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<bool> Exists(int? id)
    {
        if (id == null) return false;
        return await _context.Residence.AnyAsync(r => r.Id == id);
    }

    public async Task<Residence> Create(Residence residence)
    {
        var consortiumId = _tenantContext.GetCurrentConsortiumId();
        residence.ConsortiumId = consortiumId;

        _context.Residence.Add(residence);
        await _context.SaveChangesAsync();
        return residence;
    }

    public async Task<Residence?> GetById(int id)
    {
        return await _context.Residence.FindAsync(id);
    }

    public async Task<List<Residence>> GetResidencesAsync()
    {
        return await _context.Residence
            .Include(r => r.Users)
            .ToListAsync();
    }

    public async Task<IEnumerable<Residence>> GetAllResidencesByConsortiumWithOwner()
    {
        return await _context.Residence
             .Include(r => r.Users).ThenInclude(u => u.Role)
             .Include(r => r.Users).ThenInclude(u => u.NotificationPreference)
             .Where(r => r.Users.Any())
             .ToListAsync();
    }

    public async Task<IEnumerable<Residence>> GetResidenceByUserId(int userId) {

        return await _context.Residence
            .Include(r => r.Users)
            .Where(r => r.Users.Any(u => u.Id == userId))
            .ToListAsync();
    }

    public  Task UpdateExpense(Residence residence)
    {
         _context.Residence.Update(residence);
        return  _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByResidenceIdAsync(int residenceId, DateTime date)
    {
        var inicio = date.AddMonths(-1);

        var fin = date;

        var response = await _context.Residence
            .Where(r => r.Id == residenceId)
            .SelectMany(r => r.Invoices)
            .Where(i => i.CreatedAt >= inicio && i.CreatedAt <= fin)
            .ToListAsync();

        return response;
    }
    public async Task<Residence?> GetByIdWithoutFilters(int id)
    {
        return await _context.Residence
            .IgnoreQueryFilters()
            .Include(r => r.Consortium)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsWithoutFilters(int id)
    {
        return await _context.Residence
            .IgnoreQueryFilters()
            .AnyAsync(r => r.Id == id);
    }
}