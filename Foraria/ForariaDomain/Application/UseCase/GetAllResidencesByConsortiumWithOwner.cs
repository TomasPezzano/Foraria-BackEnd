using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllResidencesByConsortiumWithOwner
{
    Task<IEnumerable<Residence>> ExecuteAsync(int consortiumId);
}

public class GetAllResidencesByConsortiumWithOwner : IGetAllResidencesByConsortiumWithOwner
{

    private readonly IResidenceRepository _residenceRepository;

    public GetAllResidencesByConsortiumWithOwner(IResidenceRepository residenceRepository)
    {
        _residenceRepository = residenceRepository;
    }

    public async Task<IEnumerable<Residence>> ExecuteAsync(int consortiumId)
    {
        return await _residenceRepository.GetAllResidencesByConsortiumWithOwner(consortiumId);
    }
}
