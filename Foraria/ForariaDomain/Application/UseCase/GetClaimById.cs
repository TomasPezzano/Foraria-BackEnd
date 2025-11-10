using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetClaimById
{
    Task<Claim> Execute(int id);
}
public class GetClaimById : IGetClaimById
{

    public readonly IClaimRepository _claimRepository;


    public GetClaimById(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<Claim> Execute(int id)
    {
        return await _claimRepository.GetById(id);
    }
}
