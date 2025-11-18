using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Services;

public interface IContractExpirationService
{
    void CheckAndUpdateExpiration(SupplierContract contract);
    void CheckAndUpdateExpirations(List<SupplierContract> contracts);
}

public class ContractExpirationService : IContractExpirationService
{
    public void CheckAndUpdateExpiration(SupplierContract contract)
    {
        if (contract.Active && contract.EndDate < DateTime.Now)
        {
            contract.Active = false;
        }
    }

    public void CheckAndUpdateExpirations(List<SupplierContract> contracts)
    {
        foreach (var contract in contracts)
        {
            CheckAndUpdateExpiration(contract);
        }
    }
}
