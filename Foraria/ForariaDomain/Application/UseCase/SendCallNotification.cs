using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ISendCallNotification
{
    Task ExecuteForNewCallAsync(int callId);
    Task ExecuteForCallReminderAsync(int callId);
    Task ExecuteForCallCancelledAsync(int callId);
}

public class SendCallNotification : ISendCallNotification
{
    private readonly ICallRepository _callRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public SendCallNotification(
        ICallRepository callRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _callRepository = callRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task ExecuteForNewCallAsync(int callId)
    {
        var call = _callRepository.GetById(callId);
        if (call == null)
        {
            throw new KeyNotFoundException($"No se encontró la reunión con ID {callId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(call.ConsortiumId);

        // Notificar a todos los usuarios del consorcio
        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            u.Role.Description == "Inquilino" ||
            u.Role.Description == "Consorcio" ||
            u.Role.Description == "Administrador"
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "📞 Nueva Reunión Virtual";
        var body = $"Se ha programado una nueva reunión virtual para el {call.StartedAt:dd/MM/yyyy HH:mm}. ¡No te la pierdas!";

        var metadata = new Dictionary<string, string>
        {
            { "callId", call.Id.ToString() },
            { "startedAt", call.StartedAt.ToString("yyyy-MM-dd HH:mm:ss") },
            { "createdBy", call.CreatedByUserId.ToString() }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.MeetingCreated,
            title: title,
            body: body,
            relatedEntityId: call.Id,
            relatedEntityType: "Call",
            metadata: metadata
        );
    }

    public async Task ExecuteForCallReminderAsync(int callId)
    {
        var call = _callRepository.GetById(callId);
        if (call == null)
        {
            throw new KeyNotFoundException($"No se encontró la reunión con ID {callId}");
        }

        // Verificar que la reunión sea mañana (±2 horas de margen)
        var tomorrow = DateTime.Now.AddDays(1);
        var timeDifference = Math.Abs((call.StartedAt - tomorrow).TotalHours);

        if (timeDifference > 2) // Si no es aproximadamente mañana, no enviar
        {
            return;
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(call.ConsortiumId);

        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            u.Role.Description == "Inquilino" ||
            u.Role.Description == "Consorcio" ||
            u.Role.Description == "Administrador"
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "⏰ Recordatorio: Reunión Mañana";
        var body = $"Recordatorio: Tienes una reunión virtual programada para mañana {call.StartedAt:dd/MM/yyyy} a las {call.StartedAt:HH:mm}.";

        var metadata = new Dictionary<string, string>
        {
            { "callId", call.Id.ToString() },
            { "startedAt", call.StartedAt.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.MeetingReminder,
            title: title,
            body: body,
            relatedEntityId: call.Id,
            relatedEntityType: "Call",
            metadata: metadata
        );
    }

    public async Task ExecuteForCallCancelledAsync(int callId)
    {
        var call = _callRepository.GetById(callId);
        if (call == null)
        {
            throw new KeyNotFoundException($"No se encontró la reunión con ID {callId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(call.ConsortiumId);

        var usersToNotify = users.Where(u =>
            u.Role.Description == "Propietario" ||
            u.Role.Description == "Inquilino" ||
            u.Role.Description == "Consorcio" ||
            u.Role.Description == "Administrador"
        ).ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "❌ Reunión Cancelada";
        var body = $"La reunión virtual programada para el {call.StartedAt:dd/MM/yyyy HH:mm} ha sido cancelada.";

        var metadata = new Dictionary<string, string>
        {
            { "callId", call.Id.ToString() },
            { "cancelledAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.MeetingCancelled,
            title: title,
            body: body,
            relatedEntityId: call.Id,
            relatedEntityType: "Call",
            metadata: metadata
        );
    }
}
