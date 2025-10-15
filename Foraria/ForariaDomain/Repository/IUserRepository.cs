using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<User> Add(User user);
    Task<bool> ExistsEmail(string email);
    Task<User?> GetById(int id);
    Task<User?> GetByEmailWithRole(string email);
    Task<int> GetTotalUsersAsync(int? consortiumId = null);

    Task<User?> GetByIdWithRole(int id);
    Task Update(User user);
    Task GetAll();
}