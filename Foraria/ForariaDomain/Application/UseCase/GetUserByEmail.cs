using Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetUserByEmail
{
    Task<User> Execute(string email);
}

public class GetUserByEmail : IGetUserByEmail
{
    public readonly IUserRepository _userRepository;


    public GetUserByEmail(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Execute(string email)
    {
        var user = await _userRepository.GetByEmail(email);
        if (user is null)
            throw new InvalidOperationException($"User with email '{email}' not found.");
        return user;
    }
}
