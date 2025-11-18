using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetSupplierById
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierById(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public async Task<Supplier?> Execute(int supplierId)
    {
        return await _supplierRepository.GetById(supplierId);
    }

}
