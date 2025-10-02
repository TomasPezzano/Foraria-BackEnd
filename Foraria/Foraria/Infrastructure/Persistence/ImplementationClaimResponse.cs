using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Infrastructure.Persistence;

public class ImplementationClaimResponse : IClaimResponseRepository
{

    private readonly ForariaContext _context;
    public ImplementationClaimResponse(ForariaContext context)
    {
        _context = context;
    }
    public async Task Add(ClaimResponse claimResponse)
    {
        _context.ClaimsResponse.Add(claimResponse);
        _context.SaveChanges();
    }
    
}
