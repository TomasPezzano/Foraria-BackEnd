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
    public void Add(Claim claim)
    {
        _context.Claims.Add(claim);
        _context.SaveChanges();
    }

    public List<Claim> GetAll()
    {
        return _context.Claims
                   .Include(c => c.ClaimResponse)
                   .ThenInclude(cr => cr.User)
                   .ToList();
    }

    public void Update(Claim claim)
    {
        _context.Claims.Update(claim);
        _context.SaveChanges();
    }
    public Claim? GetById(int id)
    {
        return _context.Claims.FirstOrDefault(c => c.Id == id);
    }
}