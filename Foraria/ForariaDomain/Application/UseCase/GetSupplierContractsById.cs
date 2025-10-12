using ForariaDomain.Repository;
using ForariaDomain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public class GetSupplierContractsById
{

    private readonly ISupplierContractRepository _contractRepository;
    private readonly IContractExpirationService _expirationService;

    public GetSupplierContractsById(
        ISupplierContractRepository contractRepository,
        IContractExpirationService expirationService)
    {
        _contractRepository = contractRepository;
        _expirationService = expirationService;
    }

    public List<SupplierContract> Execute(int supplierId)
    {
        var contracts = _contractRepository.GetBySupplierId(supplierId);

        var contractsToUpdate = new List<SupplierContract>();

        foreach (var contract in contracts)
        {
            var wasActive = contract.Active;
            _expirationService.CheckAndUpdateExpiration(contract);

            if (wasActive && !contract.Active)
            {
                contractsToUpdate.Add(contract);
            }
        }

        foreach (var contract in contractsToUpdate)
        {
            _contractRepository.Update(contract);
        }

        return contracts;
    }
}
