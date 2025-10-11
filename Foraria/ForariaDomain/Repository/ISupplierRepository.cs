using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface ISupplierRepository
{
    Supplier Create(Supplier supplier);
    void Delete(int supplierId);
    Supplier? GetById(int supplierId);
}
