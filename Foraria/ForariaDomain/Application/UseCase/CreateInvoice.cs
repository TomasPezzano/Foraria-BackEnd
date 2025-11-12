using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;
public interface ICreateInvoice
{
    Task<Invoice> Execute(Invoice invoice);
}
public class CreateInvoice : ICreateInvoice
{
    public readonly IInvoiceRepository _invoiceRepository;

    public CreateInvoice(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Invoice> Execute(Invoice invoice)
    {
        var createdInvoice = await _invoiceRepository.CreateInvoice(invoice);
        return createdInvoice;
    }
}
