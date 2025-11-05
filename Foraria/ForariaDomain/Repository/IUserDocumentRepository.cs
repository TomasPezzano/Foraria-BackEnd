using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IUserDocumentRepository
{
    Task Add(UserDocument UserDocument);

    Task<List<UserDocument>> GetAll();
    Task<UserDocument?> GetById(int id);
    void Update(UserDocument userDocument);
    Task<List<UserDocument>> GetByCategoryAsync(string category, int? userId = null);
    Task<DateTime?> GetLastUploadDateAsync(int? userId = null);
    Task<UserDocument> GetStatsAsync(int? userId = null);
}
