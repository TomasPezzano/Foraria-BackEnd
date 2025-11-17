using Foraria.Domain.Repository;
using ForariaDomain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetTotalOwnerUsers
{
    Task<int> ExecuteAsync();
}

public class GetTotalOwnerUsers : IGetTotalOwnerUsers
{
    private readonly IUserRepository _userRepository;

    public GetTotalOwnerUsers(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<int> ExecuteAsync()
    {
        return await _userRepository.GetTotalOwnerUsersAsync();
    }
}