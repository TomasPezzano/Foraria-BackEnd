using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ICreateSupplier
{
    Supplier Execute(Supplier supplier);
}
public class CreateSupplier : ICreateSupplier
{
    private readonly ISupplierRepository _supplierRepository;
    public CreateSupplier(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public Supplier Execute(Supplier supplier)
    {
        ValidateCuit(supplier.Cuit);
        return _supplierRepository.Create(supplier);
    }

    private void ValidateCuit(string cuit)
    {

        var cleanCuit = cuit.Replace("-", "");
        if (cleanCuit.Length != 11 || !cleanCuit.All(char.IsDigit))
            throw new ArgumentException("El CUIT debe tener 11 dígitos");
    }

}
