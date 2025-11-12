using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetTotalUsers
{
    private readonly IUserRepository _repository;

    public GetTotalUsers(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> ExecuteAsync(int? consortiumId = null)
    {
        return await _repository.GetTotalUsersAsync(consortiumId);
    }
}
