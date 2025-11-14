using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;
public interface IGetActiveContractsSupplierCount
{
    Task<int> ExecuteAsync(int supplierId);
}
public class GetActiveContractsSupplierCount : IGetActiveContractsSupplierCount
{
    private readonly ISupplierContractRepository _supplierContractRepository;

    public GetActiveContractsSupplierCount(ISupplierContractRepository supplierContractRepository)
    {
        _supplierContractRepository = supplierContractRepository;
    }

    public async Task<int> ExecuteAsync(int consortiumId)
    {
        return await _supplierContractRepository.GetActiveContractsCount(consortiumId);
    }
}
