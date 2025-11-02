using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IResidenceRepository
{
    Task<bool> Exists(int? id);
    Task<Residence> Create(Residence residence, int consortiumId);
    Task<Residence?> GetById(int id);
    Task<List<Residence>> GetResidenceByConsortiumIdAsync(int consortiumId);
    Task<IEnumerable<Residence>> GetAllResidencesByConsortiumWithOwner(int consortiumId);
}
