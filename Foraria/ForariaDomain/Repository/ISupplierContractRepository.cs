using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface ISupplierContractRepository
{
    SupplierContract Create(SupplierContract contract);
    SupplierContract? GetById(int id);
    List<SupplierContract> GetBySupplierId(int supplierId);
    List<SupplierContract> GetActiveContractsBySupplierId(int supplierId);
    SupplierContract Update(SupplierContract contract);
}
