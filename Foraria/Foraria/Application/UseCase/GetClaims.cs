using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public class GetClaims
{

    private readonly IClaimRepository _claimRepository;
    public GetClaims(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task<List<Claim>> execute()
    {
        return await _claimRepository.GetAll();
    }
}
