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

    public GetClaims(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task<List<Claim>> Execute()
    {
        return await _claimRepository.GetAll();
    }

}
