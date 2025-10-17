using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        return await _responsibleSectorRepository.GetById(id);
    }
}
