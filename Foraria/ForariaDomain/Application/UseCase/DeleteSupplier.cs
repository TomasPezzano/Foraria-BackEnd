using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IDeleteSupplier
{
    bool Execute(int supplierId);
}

public class DeleteSupplier : IDeleteSupplier
{
    private readonly ISupplierRepository _supplierRepository;

    public DeleteSupplier(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public bool Execute(int supplierId)
    {
        var supplier = _supplierRepository.GetById(supplierId);

        if (supplier == null)
            throw new KeyNotFoundException($"El proveedor con ID {supplierId} no existe");

        if (supplier.Contracts?.Any(c => c.Active) == true)
            throw new InvalidOperationException("No se puede eliminar un proveedor con contratos activos");

        _supplierRepository.Delete(supplierId);
        return true;
    }
}
