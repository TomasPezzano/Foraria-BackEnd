using ForariaDomain;
using ForariaDomain.Models;

namespace Foraria.Domain.Repository;

public interface IUserDocumentRepository
{
    Task Add(UserDocument UserDocument);

    Task<List<UserDocument>> GetAll();
    Task<UserDocument?> GetById(int id);
    void Update(UserDocument userDocument);
    Task<List<UserDocument>> GetByCategoryAsync(string category, int? userId = null);
    Task<DateTime?> GetLastUploadDateAsync(int? userId = null);
    Task<UserDocumentStatsResult> GetStatsAsync(int? userId = null);
}
