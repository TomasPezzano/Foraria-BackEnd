using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface IConsortiumRepository
{
    Task<Consortium> FindById(int id);

    Task<bool> Exists(int consortiumId);

    Task AssignAdministrator(int consortiumId, int administratorId);

    Task<bool> HasAdministrator(int consortiumId);

    Task<int?> GetConsortiumIdByAdministrator(int administratorId);

    Task Update(Consortium consortium);
    Task<Consortium?> FindByIdWithoutFilters(int consortiumId);
    Task<bool> ExistsWithoutFilters(int consortiumId);
    Task<List<Consortium>> GetConsortiumsByAdministrator(int administratorId);
    Task<bool> IsAdministratorAssigned(int consortiumId, int administratorId);
}
