using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IResponsibleSectorRepository
{
    ResponsibleSector? GetById(int id);
}
