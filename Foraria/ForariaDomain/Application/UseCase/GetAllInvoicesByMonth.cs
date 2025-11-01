using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllInvoicesByMonth
{
    Task<IEnumerable<Invoice>> Execute(DateTime date);
}
public class GetAllInvoicesByMonth : IGetAllInvoicesByMonth
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesByMonth(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Invoice>> Execute(DateTime inicio)
    {
        var invoices = await _invoiceRepository.GetAllInvoicesByMonth(inicio);
        return invoices;
    }

}
