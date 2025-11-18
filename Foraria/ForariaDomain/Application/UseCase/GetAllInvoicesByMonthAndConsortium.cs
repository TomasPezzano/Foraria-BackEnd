using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface IGetAllInvoicesByMonthAndConsortium
{
    Task<IEnumerable<Invoice>> Execute(DateTime date);
}
public class GetAllInvoicesByMonthAndConsortium : IGetAllInvoicesByMonthAndConsortium
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesByMonthAndConsortium(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Invoice>> Execute(DateTime inicio)
    {
        var invoices = await _invoiceRepository.GetAllInvoicesByMonthAndConsortium(inicio);
        return invoices;
    }

}
