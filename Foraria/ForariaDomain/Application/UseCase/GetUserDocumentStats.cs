using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Models;

namespace Foraria.Application.UseCase;

public class GetUserDocumentStats
{
    private readonly IUserDocumentRepository _repository;

    public GetUserDocumentStats(IUserDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDocumentStatsResult> ExecuteAsync(int? userId = null)
    {
        return await _repository.GetStatsAsync(userId);
    }
}
