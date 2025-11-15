using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllSupplier
{
    Task<List<Supplier>> Execute(int consortiumId);
}

public class GetAllSupplier : IGetAllSupplier
{
    private readonly ISupplierRepository _supplierRepository;
    public GetAllSupplier(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public async Task<List<Supplier>> Execute(int consortiumId)
    {
        return await _supplierRepository.GetAll(consortiumId);
    }
}
