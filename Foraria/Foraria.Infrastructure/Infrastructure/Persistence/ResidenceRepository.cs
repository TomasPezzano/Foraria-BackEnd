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
        return await _context.Residence.AnyAsync(r => r.Id == id);
    }
    public async Task<Residence> Create(Residence residence)
    {
        _context.Residence.Add(residence);
        await _context.SaveChangesAsync();
        return residence;
    }
    public async Task<Residence?> GetById(int id)
    {
        return await _context.Residence.FindAsync(id);
    }
    public async Task<List<Residence>> GetAll()
    {
        return await _context.Residence.ToListAsync();
    }
}
