using Foraria.Domain.Repository;
using ForariaDomain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetTotalOwnerUsers
{
    Task<int> ExecuteAsync(int consortiumId);
}
public class GetTotalOwnerUsers : IGetTotalOwnerUsers
{
    private readonly IUserRepository _userRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public GetTotalOwnerUsers(
        IUserRepository userRepository,
        IConsortiumRepository consortiumRepository)
    {
        _userRepository = userRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<int> ExecuteAsync(int consortiumId)
    {
        var consortium = await _consortiumRepository.FindById(consortiumId);
        if (consortium == null)
        {
            throw new KeyNotFoundException($"El consorcio con ID {consortiumId} no existe.");
        }

        return await _userRepository.GetTotalOwnerUsersAsync(consortiumId);
    }
}
