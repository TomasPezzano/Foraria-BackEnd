using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IRoleRepository
{
    Task<Role?> GetById(int id);
    Task<bool> Exists(int id);
    Task<List<Role>> GetAll();

}
