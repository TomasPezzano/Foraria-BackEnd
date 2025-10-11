using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ICreateSupplierContract
{
    SupplierContract Execute(SupplierContract contract);
}
public class CreateSupplierContract : ICreateSupplierContract
{
    private readonly ISupplierContractRepository _contractRepository;
    private readonly ISupplierRepository _supplierRepository;

    public CreateSupplierContract(
        ISupplierContractRepository contractRepository,
        ISupplierRepository supplierRepository)
    {
        _contractRepository = contractRepository;
        _supplierRepository = supplierRepository;
    }

    public SupplierContract Execute(SupplierContract contract)
    {
        var supplier = _supplierRepository.GetById(contract.SupplierId);
        if (supplier == null)
        {
            throw new ArgumentException($"El proveedor con ID {contract.SupplierId} no existe.");
        }

        if (!supplier.Active)
        {
            throw new InvalidOperationException($"El proveedor '{supplier.CommercialName}' está inactivo y no puede tener contratos nuevos.");
        }

        if (contract.EndDate <= contract.StartDate)
        {
            throw new ArgumentException("La fecha de vencimiento debe ser posterior a la fecha de inicio.");
        }

        if (contract.StartDate < DateTime.UtcNow.AddYears(-10))
        {
            throw new ArgumentException("La fecha de inicio no puede ser mayor a 10 años en el pasado.");
        }

        if (contract.EndDate < DateTime.UtcNow)
        {
            throw new ArgumentException("No se puede crear un contrato con fecha de vencimiento en el pasado.");
        }

        if (contract.MonthlyAmount <= 0)
        {
            throw new ArgumentException("El monto mensual debe ser mayor a 0.");
        }

        var existingContracts = _contractRepository.GetBySupplierId(contract.SupplierId);
        if (existingContracts.Any(c => c.Name.Equals(contract.Name, StringComparison.OrdinalIgnoreCase) && c.Active))
        {
            throw new InvalidOperationException($"Ya existe un contrato activo con el nombre '{contract.Name}' para este proveedor.");
        }

        contract.Active = true;
        contract.CreatedAt = DateTime.UtcNow;

        return _contractRepository.Create(contract);
    }
}
