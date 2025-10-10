using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<User> Add(User user);
    Task<bool> ExistsEmail(string email);
    Task<User?> GetById(int id);
    Task<User?> GetByEmailWithRole(string email);

}

