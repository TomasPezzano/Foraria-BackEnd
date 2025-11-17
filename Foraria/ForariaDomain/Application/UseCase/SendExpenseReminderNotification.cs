using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ISendExpenseReminderNotification
{
    Task ExecuteAsync(int expenseId);
}
public class SendExpenseReminderNotification : ISendExpenseReminderNotification
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    public SendExpenseReminderNotification(
        IExpenseRepository expenseRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _expenseRepository = expenseRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }
    public async Task ExecuteAsync(int expenseId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
        {
            throw new KeyNotFoundException($"No se encontró la expensa con ID {expenseId}");
        }
        var daysUntilExpiration = (expense.ExpirationDate - DateTime.UtcNow).Days;
        if (daysUntilExpiration < 0 || daysUntilExpiration > 1)
        {
            return;
        }
        var users = await _userRepository.GetUsersByConsortiumIdAsync();
        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            (u.Role.Description == "Inquilino" && u.HasPermission)
        ).ToList();
        if (!usersToNotify.Any())
        {
            return;
        }
        var title = "⏰ Recordatorio de Pago";
        var body = daysUntilExpiration == 0
            ? $"Tu expensa '{expense.Description}' vence HOY. Monto: ${expense.TotalAmount:N2}"
            : $"Tu expensa '{expense.Description}' vence MAÑANA. Monto: ${expense.TotalAmount:N2}";
        var metadata = new Dictionary<string, string>
        {
            { "expenseId", expense.Id.ToString() },
            { "amount", expense.TotalAmount.ToString() },
            { "expirationDate", expense.ExpirationDate.ToString("yyyy-MM-dd") },
            { "consortiumId", expense.ConsortiumId.ToString() }
        };
        var userIds = usersToNotify.Select(u => u.Id).ToList();
        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.ExpenseReminder,
            title: title,
            body: body,
            relatedEntityId: expense.Id,
            relatedEntityType: "Expense",
            metadata: metadata
        );
    }
}
