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
    public List<Claim> execute()
    {
        return _claimRepository.GetAll();
    }
}
