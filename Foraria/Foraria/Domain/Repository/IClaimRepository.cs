using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IClaimRepository
{
    void Add(Claim claim);

    List<Claim> GetAll();

    void Update(Claim claim);

    Claim? GetById(int id);

}
