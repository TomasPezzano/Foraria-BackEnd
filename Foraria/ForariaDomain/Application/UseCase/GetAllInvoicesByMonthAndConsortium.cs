using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllInvoicesByMonthAndConsortium
{
    Task<IEnumerable<Invoice>> Execute(DateTime date, int consortiumId);
}
public class GetAllInvoicesByMonthAndConsortium : IGetAllInvoicesByMonthAndConsortium
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesByMonthAndConsortium(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Invoice>> Execute(DateTime inicio, int consortiumId)
    {
        var invoices = await _invoiceRepository.GetAllInvoicesByMonthAndConsortium(inicio, consortiumId);
        return invoices;
    }

}
