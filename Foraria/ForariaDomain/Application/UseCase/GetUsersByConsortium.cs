using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using Microsoft.IdentityModel.Tokens;


namespace ForariaDomain.Application.UseCase;

public interface IGetUsersByConsortium
{
    Task<List<User>> ExecuteAsync(int consortiumId);
}

public class GetUsersByConsortium : IGetUsersByConsortium
{
    private readonly IUserRepository _userRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public GetUsersByConsortium(
        IUserRepository userRepository,
        IConsortiumRepository consortiumRepository)
    {
        _userRepository = userRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<List<User>> ExecuteAsync(int consortiumId)
    {
        if (consortiumId <= 0)
        {
            throw new ArgumentException("El ID del consorcio debe ser mayor a 0.", nameof(consortiumId));
        }

        var consortium = await _consortiumRepository.FindById(consortiumId);
        if (consortium == null)
        {
            throw new KeyNotFoundException($"El consorcio con ID {consortiumId} no existe.");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(consortiumId);

        if (users.IsNullOrEmpty())
        {
            throw new KeyNotFoundException($"El consorcio con ID {consortiumId} no tiene usuarios asignados");
        }

        return users;
    }
}