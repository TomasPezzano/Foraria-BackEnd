using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllInvoices
{
    Task<IEnumerable<Invoice>> Execute();
}
public class GetAllInvoices : IGetAllInvoices
{
    public readonly IInvoiceRepository _invoiceRepository;
    public GetAllInvoices(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }
    public async Task<IEnumerable<Invoice>> Execute()
    {
        var invoices = await _invoiceRepository.GetAllInvoices();
        return invoices;
    }
}
