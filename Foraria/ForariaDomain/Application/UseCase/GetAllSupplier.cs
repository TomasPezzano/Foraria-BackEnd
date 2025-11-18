using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllSupplier
{
    Task<List<Supplier>> Execute();
}

public class GetAllSupplier : IGetAllSupplier
{
    private readonly ISupplierRepository _supplierRepository;
    public GetAllSupplier(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public async Task<List<Supplier>> Execute()
    {
        return await _supplierRepository.GetAll();
    }
}
