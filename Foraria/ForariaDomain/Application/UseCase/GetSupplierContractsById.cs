using ForariaDomain.Repository;
using ForariaDomain.Services;


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

    public async Task<List<SupplierContract>> Execute(int supplierId)
    {
        var contracts = await _contractRepository.GetBySupplierId(supplierId);

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
