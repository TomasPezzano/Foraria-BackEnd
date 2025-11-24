using ForariaDomain.Repository;
using ForariaDomain.Services;


namespace ForariaDomain.Application.UseCase;

public class GetSupplierContractById
{
    private readonly ISupplierContractRepository _contractRepository;
    private readonly IContractExpirationService _expirationService;

    public GetSupplierContractById(ISupplierContractRepository contractRepository, IContractExpirationService expirationService)
    {
        _contractRepository = contractRepository;
        _expirationService = expirationService;
    }

    public async Task<SupplierContract?> Execute(int contractId)
    {
        var contract = await _contractRepository.GetById(contractId);

        if (contract == null)
            return null;

        var wasActive = contract.Active;
        _expirationService.CheckAndUpdateExpiration(contract);

        if (wasActive && !contract.Active)
        {
            _contractRepository.Update(contract);
        }

        return contract;
    }
}