using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ClaimImplementation : IClaimRepository
{
    private readonly ForariaContext _context;
    public ClaimImplementation(ForariaContext context)
    {
        _context = context;
    }
    public async Task Add(Claim claim)
    {
        _context.Claims.Add(claim);
        await  _context.SaveChangesAsync();
    }

    public async Task<List<Claim>> GetAll()
    {
        return  await _context.Claims
                   .Include(c => c.ClaimResponse)
                   .ThenInclude(cr => cr.User)
                   .ToListAsync();
    }

    public async Task Update(Claim claim)
    {
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();
    }
    public async Task<Claim?> GetById(int id)
    {
        return await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
    }
}