using Foraria.Domain.Repository;
using ForariaDomain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetTotalTenantUsers
{
    Task<int> ExecuteAsync();
}

public class GetTotalTenantUsers : IGetTotalTenantUsers
{
    private readonly IUserRepository _userRepository;

    public GetTotalTenantUsers(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<int> ExecuteAsync()
    {
        return await _userRepository.GetTotalUsersByTenantIdAsync();
    }
}
