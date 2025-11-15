using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface ISupplierContractRepository
{
    Task<SupplierContract> Create(SupplierContract contract);
    Task<SupplierContract?> GetById(int id);
    Task<List<SupplierContract>> GetBySupplierId(int supplierId);
    Task<List<SupplierContract>> GetActiveContractsBySupplierId(int supplierId);
    Task<SupplierContract> Update(SupplierContract contract);
    Task<int> GetActiveContractsCount(int consortiumId);
}
