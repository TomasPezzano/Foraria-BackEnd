using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Infrastructure.Persistence;

public class ImplementationResponsibleSector : IResponsibleSectorRepository
{
    private readonly ForariaContext _context;
    public ImplementationResponsibleSector(ForariaContext context)
    {
        _context = context;
    }
    public ResponsibleSector? GetById(int id)
    {
        return _context.ResponsibleSectors.FirstOrDefault(rs => rs.Id == id);
    }

}
