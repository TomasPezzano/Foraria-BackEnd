using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;
public interface IGetClaims
{
    Task<List<Claim>> Execute();
}
public class GetClaims : IGetClaims
{

    private readonly IClaimRepository _claimRepository;
    private readonly IGetClaims _getClaims;
    public GetClaims(IClaimRepository claimRepository, IGetClaims getClaims)
    {
        _claimRepository = claimRepository;
        _getClaims = getClaims;
    }
    public async Task<List<Claim>> Execute()
    {
        return await _claimRepository.GetAll();
    }

}
