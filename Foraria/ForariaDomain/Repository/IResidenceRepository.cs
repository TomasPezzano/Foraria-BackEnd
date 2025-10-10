using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IResidenceRepository
{
    Task<bool> Exists(int? id);
    Task<Residence> Create(Residence residence);
    Task<Residence?> GetById(int id);
    Task<List<Residence>> GetAll();

}
