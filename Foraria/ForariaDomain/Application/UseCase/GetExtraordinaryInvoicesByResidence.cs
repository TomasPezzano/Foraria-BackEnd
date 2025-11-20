using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetExtraordinaryInvoicesByResidence
{
    Task<List<Invoice>> ExecuteAsync(int residenceId);
}

public class GetExtraordinaryInvoicesByResidence : IGetExtraordinaryInvoicesByResidence
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetExtraordinaryInvoicesByResidence(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<List<Invoice>> ExecuteAsync(int residenceId)
    {
        if (residenceId <= 0)
            throw new ArgumentException("El ID de la residencia debe ser un número positivo.");
        var extraordinaryInvoices = await _invoiceRepository.GetExtraordinaryInvoicesByResidenceIdAsync(residenceId);
        return extraordinaryInvoices.ToList();
    }
}
