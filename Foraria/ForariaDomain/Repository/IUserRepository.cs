using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<User> Add(User user);
    Task<bool> ExistsEmail(string email);
    Task<User?> GetById(int id);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailWithRole(string email);
    Task<int> GetTotalUsersAsync();
    Task<int> GetTotalUsersByTenantIdAsync();
    Task<int> GetTotalOwnerUsersAsync();
    Task<User?> GetByIdWithRole(int id);
    Task Update(User user);
    Task<int> GetAllInNumber();
    Task<List<User>> GetUsersByConsortiumIdAsync();
    Task<bool> ExistsUserWithRoleInResidence(int residenceId, string roleDescription);
    List<int> GetConsortiumIdsByUserId(int userId);
    Task<User?> GetByEmailWithoutFilters(string email);
    Task<User?> GetByIdWithoutFilters(int id);
    Task<IEnumerable<User>> GetUsersByConsortiumIdAsync(int? consortiumId);

}