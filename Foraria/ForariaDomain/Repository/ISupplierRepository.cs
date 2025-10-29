using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface ISupplierRepository
{
    Task<Supplier> Create(Supplier supplier);
    void Delete(int supplierId);
    List <Supplier> GetAll();
    Supplier? GetById(int supplierId);
}
