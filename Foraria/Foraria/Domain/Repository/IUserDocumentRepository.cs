using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IUserDocumentRepository
{
    Task Add(UserDocument UserDocument);

    Task<List<UserDocument>> GetAll();
}
