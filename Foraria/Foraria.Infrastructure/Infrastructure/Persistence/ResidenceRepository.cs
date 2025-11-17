using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ResidenceRepository : IResidenceRepository
{
    private readonly ForariaContext _context;

    public ResidenceRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<bool> Exists(int? id)
    {
        if (id == null) return false;
        return await _context.Residence.AnyAsync(r => r.Id == id);
    }

    public async Task<Residence> Create(Residence residence, int consortiumId)
    {
        residence.ConsortiumId = consortiumId;
        _context.Residence.Add(residence);
        await _context.SaveChangesAsync();
        return residence;
    }

    public async Task<Residence?> GetById(int id)
    {
        return await _context.Residence.FindAsync(id);
    }

    public async Task<List<Residence>> GetResidenceByConsortiumIdAsync(int consortiumId)
    {
        return await _context.Residence
            .Include(r => r.Users)
            .Where(r => r.ConsortiumId == consortiumId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Residence>> GetAllResidencesByConsortiumWithOwner(int consortiumId)
    {
        return await _context.Residence
             .Include(r => r.Users)
             .Where(r => r.ConsortiumId == consortiumId && r.Users.Count() > 0)
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