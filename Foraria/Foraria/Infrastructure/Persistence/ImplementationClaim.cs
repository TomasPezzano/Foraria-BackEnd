using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ImplementationClaim : IClaimRepository
{
    private readonly ForariaContext _context;
    public ImplementationClaim(ForariaContext context)
    {
        _context = context;
    }
    public async Task Add(Claim claim)
    {
        _context.Claims.Add(claim);
        _context.SaveChanges();
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
        _context.SaveChanges();
    }
    public async Task<Claim?> GetById(int id)
    {
        return await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
    }
}