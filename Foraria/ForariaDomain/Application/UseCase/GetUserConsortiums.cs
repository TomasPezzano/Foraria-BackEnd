using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetUserConsortiums
{
    Task<List<Consortium>> Execute(int userId);
}

public class GetUserConsortiums : IGetUserConsortiums
{
    private readonly IUserRepository _userRepository;
    private readonly IConsortiumRepository _consortiumRepository;

    public GetUserConsortiums(
        IUserRepository userRepository,
        IConsortiumRepository consortiumRepository)
    {
        _userRepository = userRepository;
        _consortiumRepository = consortiumRepository;
    }

    public async Task<List<Consortium>> Execute(int userId)
    {
        var consortiumIds = _userRepository.GetConsortiumIdsByUserId(userId);

        if (consortiumIds.Count == 0)
            return new List<Consortium>();

        var consortiums = new List<Consortium>();

        foreach (var consortiumId in consortiumIds)
        {
            var consortium = await _consortiumRepository.FindByIdWithoutFilters(consortiumId);
            if (consortium != null)
            {
                consortiums.Add(consortium);
            }
        }

        return consortiums.OrderBy(c => c.Name).ToList();
    }
}
