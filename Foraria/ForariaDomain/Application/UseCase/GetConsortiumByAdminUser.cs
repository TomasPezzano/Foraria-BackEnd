using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;
public interface IGetConsortiumByAdminUser
{
    Task<int?> Execute(int id);
}

public class GetConsortiumByAdminUser : IGetConsortiumByAdminUser
{
    private readonly IConsortiumRepository _consortiumRepository;
    public GetConsortiumByAdminUser(
        IConsortiumRepository consortiumRepository)
    {
        _consortiumRepository = consortiumRepository;
    }
    public async Task<int?> Execute(int id)
    {
        var consortium = await _consortiumRepository.GetByAdminUserId(id);
        if (consortium == null)
            throw new InvalidOperationException($"Consortium for admin user with ID '{id}' not found.");
        return consortium;
    }
}
