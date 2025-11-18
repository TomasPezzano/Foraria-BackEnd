using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using Microsoft.IdentityModel.Tokens;


namespace ForariaDomain.Application.UseCase;

public interface IGetUsersByConsortium
{
    Task<List<User>> ExecuteAsync();
}

public class GetUsersByConsortium : IGetUsersByConsortium
{
    private readonly IUserRepository _userRepository;

    public GetUsersByConsortium(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> ExecuteAsync()
    {
        var users = await _userRepository.GetUsersByConsortiumIdAsync();

        if (users.IsNullOrEmpty())
        {
            return new List<User>(); 
        }

        return users;
    }
}