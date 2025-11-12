using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetResponsibleSectorById
{
    Task<ResponsibleSector> Execute(int id);
}
public class GetResponsibleSectorById : IGetResponsibleSectorById
{
    public readonly IResponsibleSectorRepository _responsibleSectorRepository;
    public GetResponsibleSectorById(IResponsibleSectorRepository responsibleSectorRepository)
    {
        _responsibleSectorRepository = responsibleSectorRepository;
    }
    public async Task<ResponsibleSector> Execute(int id)
    {
        var sector = await _responsibleSectorRepository.GetById(id);
        if (sector is null)
            throw new InvalidOperationException($"ResponsibleSector with id {id} not found.");
        return sector;
    }
}
