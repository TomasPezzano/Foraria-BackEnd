using ForariaDomain;

namespace Foraria.Domain.Repository;

public interface IResidenceRepository
{
    Task<bool> Exists(int? id);
    Task<Residence> Create(Residence residence, int consortiumId);
    Task<Residence?> GetById(int id);
    Task<List<Residence>> GetResidenceByConsortiumIdAsync(int consortiumId);
    Task<IEnumerable<Residence>> GetAllResidencesByConsortiumWithOwner();

    Task<IEnumerable<Residence>> GetResidenceByUserId(int userId);
    Task UpdateExpense(Residence residence);
    Task<Residence?> GetByIdWithoutFilters(int id);
    Task<bool> ExistsWithoutFilters(int id);
    Task<IEnumerable<Invoice>> GetInvoicesByResidenceIdAsync(int id, DateTime date);
}
