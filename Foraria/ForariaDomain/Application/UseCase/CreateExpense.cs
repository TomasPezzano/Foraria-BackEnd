using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using ForariaDomain.Services;

namespace ForariaDomain.Application.UseCase;

public interface ICreateExpense
{
    Task<Expense> ExecuteAsync(string date);
}
public class CreateExpense : ICreateExpense
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IResidenceRepository _residenceRepository;
    private readonly IGetAllInvoicesByMonthAndConsortium _getAllInvoicesByMonthAndConsortium;
    private readonly IGetConsortiumById _getConsortiumById;
    private readonly IGetAllResidencesByConsortiumWithOwner _getAllResidencesByConsortiumWithOwner;
    private readonly ITenantContext _tenantContext;

    public CreateExpense(IExpenseRepository expenseRepository, IGetAllInvoicesByMonthAndConsortium getAllInvoicesByMonthAndConsortium, 
        IGetConsortiumById getConsortiumById, IInvoiceRepository invoiceRepository, 
        IGetAllResidencesByConsortiumWithOwner getAllResidencesByConsortiumWithOwner,
        IResidenceRepository residenceRepository, ITenantContext tenantContext)
    {
        _expenseRepository = expenseRepository;
        _getAllInvoicesByMonthAndConsortium = getAllInvoicesByMonthAndConsortium;
        _getConsortiumById = getConsortiumById;
        _invoiceRepository = invoiceRepository;
        _getAllResidencesByConsortiumWithOwner = getAllResidencesByConsortiumWithOwner;
        _residenceRepository = residenceRepository;
        _tenantContext = tenantContext;
    }
    public async Task<Expense> ExecuteAsync(string date)
    {
        DateTime inicio;
        try
        {
            var partes = date.Split('-');
            if (partes.Length != 2)
                throw new FormatException();

            int anio = int.Parse(partes[0]);
            int mesNumero = int.Parse(partes[1]);

            inicio = new DateTime(anio, mesNumero, 1);
        }
        catch
        {
            throw new FormatException("El formato de la fecha es inválido. Usa 'YYYY-MM' (por ejemplo, '2025-10').");
        }

        var consortiumId = _tenantContext.GetCurrentConsortiumId();

        var consortium =  await _getConsortiumById.Execute(consortiumId);
        if (consortium == null)
            throw new KeyNotFoundException($"No se encontró ningún consorcio con ID.");


        var invoices = await _getAllInvoicesByMonthAndConsortium.Execute(inicio); 
        if (invoices == null || !invoices.Any())
            throw new InvalidOperationException($"No existen facturas registradas para el consorcio en {inicio:MMMM yyyy}.");
       
        
        var residences = await _getAllResidencesByConsortiumWithOwner.ExecuteAsync(); 
        if (residences == null || !residences.Any())
            throw new InvalidOperationException($"No existen residencias registradas para el consorcio");

        double totalAmount = invoices.Sum(i => (double)i.Amount);
        if (totalAmount <= 0)
            throw new InvalidOperationException("El total de las facturas no puede ser cero o negativo.");


        var newExpense = new Expense
        {
            Description = $"Gastos del mes {inicio.ToString("MMMM yyyy")}",
            CreatedAt = inicio.AddMonths(1),
            ExpirationDate = inicio.AddDays(15),
            TotalAmount = totalAmount,
            ConsortiumId = consortiumId,
            Consortium = consortium,
            Invoices = invoices.ToList(),
            Residences = residences.ToList()
        };

        var expense = await _expenseRepository.AddExpenseAsync(newExpense);

        foreach (var invoice in invoices)
        {
            invoice.ExpenseId = expense.Id;
            await _invoiceRepository.UpdateInvoiceAsync(invoice);
        }
        
        foreach (var residence in residences)
        {
            residence.Expenses.Add(expense);
            await _residenceRepository.UpdateExpense(residence);
        }

        return expense;
    }

}
