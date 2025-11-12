using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;
public interface IGetUserById
{
    Task<User> Execute(int id);
}

public class GetUserById : IGetUserById
{
    public readonly IUserRepository _userRepository;


    public GetUserById(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Execute(int id)
    {
        var user = await _userRepository.GetById(id);
        if (user is null)
            throw new InvalidOperationException($"User with id {id} not found.");
        return user;
    }
}
