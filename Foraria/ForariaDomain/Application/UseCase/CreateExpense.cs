using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ICreateExpense
{
    Task<Expense> ExecuteAsync(int consortiumId, string date);
}
public class CreateExpense : ICreateExpense
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IGetAllInvoicesByMonthAndConsortium _getAllInvoicesByMonthAndConsortium;
    private readonly IGetConsortiumById _getConsortiumById;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserRepository _userRepository;

    public CreateExpense(IExpenseRepository expenseRepository, IGetAllInvoicesByMonthAndConsortium getAllInvoicesByMonthAndConsortium, IGetConsortiumById getConsortiumById, IInvoiceRepository invoiceRepository, INotificationDispatcher notificationDispatcher, IUserRepository userRepository)
    {
        _expenseRepository = expenseRepository;
        _getAllInvoicesByMonthAndConsortium = getAllInvoicesByMonthAndConsortium;
        _getConsortiumById = getConsortiumById;
        _invoiceRepository = invoiceRepository;
        _notificationDispatcher = notificationDispatcher;
        _userRepository = userRepository;
    }
    public async Task<Expense> ExecuteAsync(int consortiumId, string date)
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

        var consortium =  await _getConsortiumById.Execute(consortiumId);
        if (consortium == null)
            throw new KeyNotFoundException($"No se encontró ningún consorcio con ID {consortiumId}.");


        var invoices = await _getAllInvoicesByMonthAndConsortium.Execute(inicio, consortiumId); // checkear de pasar consortium? 
        if (invoices == null || !invoices.Any())
            throw new InvalidOperationException($"No existen facturas registradas para el consorcio {consortiumId} en {inicio:MMMM yyyy}.");

        double totalAmount = invoices.Sum(i => (double)i.Amount);
        if (totalAmount <= 0)
            throw new InvalidOperationException("El total de las facturas no puede ser cero o negativo.");


        var newExpense = new Expense
        {
            Description = $"Gastos del mes {inicio.ToString("MMMM yyyy")}",
            CreatedAt = inicio.AddMonths(1),
            ExpirationDate = inicio.AddMonths(1).AddDays(15),
            TotalAmount = totalAmount,
            ConsortiumId = consortiumId,
            Consortium = consortium,
            Invoices = invoices.ToList()
        };

        var expense = await _expenseRepository.AddExpenseAsync(newExpense);

        foreach (var invoice in invoices)
        {
            invoice.ExpenseId = expense.Id;
            await _invoiceRepository.UpdateInvoiceAsync(invoice);
        }

        try
        {
            // Obtener usuarios del consorcio
            var users = await _userRepository.GetUsersByConsortiumIdAsync(consortiumId);

            // Filtrar solo Propietarios e Inquilinos con permiso
            var usersToNotify = users.Where(u =>
                u.Role.Description == "Propietario" ||
                (u.Role.Description == "Inquilino" && u.HasPermission)
            ).ToList();

            if (usersToNotify.Any())
            {
                var userIds = usersToNotify.Select(u => u.Id).ToList();

                await _notificationDispatcher.SendBatchNotificationAsync(
                    userIds: userIds,
                    type: NotificationType.ExpenseCreated,
                    title: "📋 Nueva Expensa Creada",
                    body: $"Se creó la expensa de {inicio:MMMM yyyy}. Vence el {expense.ExpirationDate:dd/MM/yyyy}. Monto: ${expense.TotalAmount:N2}",
                    relatedEntityId: expense.Id,
                    relatedEntityType: "Expense",
                    metadata: new Dictionary<string, string>
                    {
                        { "expenseId", expense.Id.ToString() },
                        { "amount", expense.TotalAmount.ToString() },
                        { "expirationDate", expense.ExpirationDate.ToString("yyyy-MM-dd") },
                        { "consortiumId", consortiumId.ToString() }
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar notificaciones: {ex.Message}");
        }

        return expense;
    }

}
