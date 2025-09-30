using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Infrastructure.Persistence;

public class ImplementationClaimRepository : IClaimRepository
{
    private readonly ForariaContext _context;
    public ImplementationClaimRepository(ForariaContext context)
    {
        _context = context;
    }
    public void Add(Claim claim)
    {
        _context.Claims.Add(claim);
        _context.SaveChanges();
    }
}
