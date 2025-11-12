using Foraria.Domain.Repository;
using ForariaDomain.Repository;


namespace ForariaDomain.Application.UseCase;

public interface IGetTotalTenantUsers
{
    Task<int> ExecuteAsync(int idConsortium);
}
public class GetTotalTenantUsers : IGetTotalTenantUsers
{
    private readonly IUserRepository _userRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public GetTotalTenantUsers(IUserRepository userRepository, IConsortiumRepository consortiumRepository)
    {
        _userRepository = userRepository;
        _consortiumRepository = consortiumRepository;
    }
    public async Task<int> ExecuteAsync(int idConsortium)
    {
        var consortium = await _consortiumRepository.FindById(idConsortium);

        if (consortium == null)
        {
            throw new KeyNotFoundException($"El consorcio con ID {idConsortium} no existe.");
        }

        return await _userRepository.GetTotalUsersByTenantIdAsync(idConsortium);
    }
}
