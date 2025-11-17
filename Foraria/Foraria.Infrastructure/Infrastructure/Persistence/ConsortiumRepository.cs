// Ubicación: Foraria.Infrastructure/Persistence/ConsortiumRepository.cs
using Foraria.Infrastructure.Persistence;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence;

public class ConsortiumRepository : IConsortiumRepository
{
    private readonly ForariaContext _context;

    public ConsortiumRepository(ForariaContext context)
    {
        _context = context;
    }

    public async Task<Consortium?> FindById(int consortiumId)
    {
        return await _context.Consortium
            .Include(c => c.Administrator)
            .FirstOrDefaultAsync(c => c.Id == consortiumId);
    }

    public async Task<bool> Exists(int consortiumId)
    {
        return await _context.Consortium.AnyAsync(c => c.Id == consortiumId);
    }

    public async Task AssignAdministrator(int consortiumId, int administratorId)
    {
        var consortium = await _context.Consortium.FindAsync(consortiumId);
        if (consortium == null)
            throw new NotFoundException($"Consorcio con ID {consortiumId} no encontrado.");

        consortium.AdministratorId = administratorId;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAdministrator(int consortiumId)
    {
        var consortium = await _context.Consortium
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == consortiumId);

        return consortium?.AdministratorId.HasValue ?? false;
    }

    public async Task<int?> GetConsortiumIdByAdministrator(int administratorId)
    {
        var consortium = await _context.Consortium
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AdministratorId == administratorId);

        return consortium?.Id;
    }

    public async Task Update(Consortium consortium)
    {
        _context.Consortium.Update(consortium);
        await _context.SaveChangesAsync();
    }
    public async Task<Consortium?> FindByIdWithoutFilters(int consortiumId)
    {
        return await _context.Consortium
            .IgnoreQueryFilters()
            .Include(c => c.Administrator)
            .FirstOrDefaultAsync(c => c.Id == consortiumId);
    }

    public async Task<bool> ExistsWithoutFilters(int consortiumId)
    {
        return await _context.Consortium
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == consortiumId);
    }

    public async Task<List<Consortium>> GetConsortiumsByAdministrator(int administratorId)
    {
        return await _context.Consortium
            .IgnoreQueryFilters()
            .Where(c => c.AdministratorId == administratorId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> IsAdministratorAssigned(int consortiumId, int administratorId)
    {
        var consortium = await _context.Consortium
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == consortiumId);

        return consortium?.AdministratorId == administratorId;
    }
}