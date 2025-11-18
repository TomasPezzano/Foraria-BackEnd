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
                   .Include(c => c.User)
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
    public async Task<int> GetPendingCountAsync()
    {
        var query = _context.Claims
            .Include(c => c.Residence)
            .AsQueryable();

        return await query.CountAsync(c => c.State == "Pending");
    }

    public async Task<Claim?> GetLatestPendingAsync()
    {
        var query = _context.Claims
            .Include(c => c.User)
            .Include(c => c.Residence)
                .ThenInclude(r => r.Consortium)
            .AsQueryable();

        return await query
            .Where(c => c.State == "Pending")
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }
}