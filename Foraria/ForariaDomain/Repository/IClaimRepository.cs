using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IClaimRepository
{
    Task Add(Claim claim);

    Task<List<Claim>> GetAll();

    Task Update(Claim claim);

    Task<Claim?> GetById(int id);
}
