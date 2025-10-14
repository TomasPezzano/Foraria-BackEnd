using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IClaimRepository
{
    Task Add(Claim claim);

    Task<List<Claim>> GetAll();

    Task Update(Claim claim);

    Task<Claim?> GetById(int id);
    Task<int> GetPendingCountAsync(int? consortiumId = null);
    Task<Claim?> GetLatestPendingAsync(int? consortiumId = null);
}
