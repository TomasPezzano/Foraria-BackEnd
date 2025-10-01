using Foraria.Domain.Repository;
using ForariaDomain;

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
        return _context.Claims.ToList();
    }

    public void Update(Claim claim)
    {
        _context.Claims.Update(claim);
        _context.SaveChanges();
    }

    public void Delete(Claim claim)
    {
        _context.Claims.Remove(claim);
        _context.SaveChanges();
    }

    public Claim? GetById(int id)
    {
        return _context.Claims.FirstOrDefault(c => c.Id == id);
    }
}