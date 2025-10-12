using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public class GetSupplierById
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierById(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public Supplier? Execute(int supplierId)
    {
        return _supplierRepository.GetById(supplierId);
    }

}
