using ForariaDomain.Repository;
using ForariaDomain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public SupplierContract? Execute(int contractId)
    {
        var contract = _contractRepository.GetById(contractId);

        if (contract == null)
            return null;

        // Verificar y actualizar si está vencido
        var wasActive = contract.Active;
        _expirationService.CheckAndUpdateExpiration(contract);

        // Si cambió de activo a inactivo, guardar en la DB
        if (wasActive && !contract.Active)
        {
            _contractRepository.Update(contract);
        }

        return contract;
    }
}