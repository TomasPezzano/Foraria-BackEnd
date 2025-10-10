using Foraria.Domain.Repository;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ResponsibleSectorImplementation : IResponsibleSectorRepository
{
    private readonly ForariaContext _context;
    public ResponsibleSectorImplementation(ForariaContext context)
    {
        _context = context;
    }
    public async Task<ResponsibleSector?> GetById(int id)
    {
        return await _context.ResponsibleSectors.FirstOrDefaultAsync(rs => rs.Id == id);
    }

}
