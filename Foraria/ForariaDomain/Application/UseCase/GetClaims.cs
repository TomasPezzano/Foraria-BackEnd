using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaDomain.Application.UseCase;
public interface IGetClaims
{
    Task<List<Claim>> Execute(int consortiumId);
}
public class GetClaims : IGetClaims
{

    private readonly IClaimRepository _claimRepository;

    public GetClaims(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }
    public async Task<List<Claim>> Execute(int consortiumId)
    {
        return await _claimRepository.GetAll(consortiumId);
    }

}
