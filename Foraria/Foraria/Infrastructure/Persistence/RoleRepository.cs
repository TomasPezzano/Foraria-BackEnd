using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class RoleRepository : IRoleRepository
{
    private readonly ForariaContext _context;

    public RoleRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetById(int id)
    {
        return await _context.Rols.FindAsync(id);
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.Rols.AnyAsync(r => r.Id == id);
    }

    public async Task<List<Role>> GetAll()
    {
        return await _context.Rols.ToListAsync();
    }
}
