using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IResponsibleSectorRepository
{
    Task<ResponsibleSector?> GetById(int id);
}
