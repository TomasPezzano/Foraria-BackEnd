using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetPendingClaimsCount
{
    private readonly IClaimRepository _repository;

    public GetPendingClaimsCount(IClaimRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> ExecuteAsync()
    {
        return await _repository.GetPendingCountAsync();
    }
}
