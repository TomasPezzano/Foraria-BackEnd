using ForariaDomain;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllSupplier
{
    List<Supplier> Execute();
}

public class GetAllSupplier : IGetAllSupplier
{
    private readonly ISupplierRepository _supplierRepository;
    public GetAllSupplier(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public List<Supplier> Execute()
    {
        return _supplierRepository.GetAll();
    }
}
