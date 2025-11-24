using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ISendClaimNotification
{
    Task ExecuteForNewClaimAsync(int claimId);
    Task ExecuteForClaimResponseAsync(int claimId);
    Task ExecuteForClaimStatusUpdateAsync(int claimId, string newStatus);
    Task ExecuteForClaimResolvedAsync(int claimId);
}

public class SendClaimNotification : ISendClaimNotification
{
    private readonly IClaimRepository _claimRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public SendClaimNotification(
        IClaimRepository claimRepository,
        IUserRepository userRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _claimRepository = claimRepository;
        _userRepository = userRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task ExecuteForNewClaimAsync(int claimId)
    {
        var claim = await _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new KeyNotFoundException($"No se encontró el reclamo con ID {claimId}");
        }

        var users = await _userRepository.GetUsersByConsortiumIdAsync(claim.ConsortiumId);


        var usersToNotify = users.ToList();

        if (!usersToNotify.Any())
        {
            return;
        }

        var title = "📝 Nuevo Reclamo Registrado";
        var body = $"Nuevo reclamo en {claim.Category}: '{claim.Title}'. Prioridad: {claim.Priority}";

        var metadata = new Dictionary<string, string>
        {
            { "claimId", claim.Id.ToString() },
            { "title", claim.Title },
            { "category", claim.Category },
            { "priority", claim.Priority },
            { "state", claim.State },
            { "consortiumId", claim.ConsortiumId.ToString() }
        };

        var userIds = usersToNotify.Select(u => u.Id).ToList();

        await _notificationDispatcher.SendBatchNotificationAsync(
            userIds: userIds,
            type: NotificationType.ClaimCreated,
            title: title,
            body: body,
            relatedEntityId: claim.Id,
            relatedEntityType: "Claim",
            metadata: metadata
        );
    }

    public async Task ExecuteForClaimResponseAsync(int claimId)
    {
        var claim = await _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new KeyNotFoundException($"No se encontró el reclamo con ID {claimId}");
        }

        if (claim.User_id == null)
        {
            return;
        }

        var title = "💬 Respuesta a tu Reclamo";
        var body = $"Tu reclamo '{claim.Title}' tiene una nueva respuesta. ¡Revísala!";

        var metadata = new Dictionary<string, string>
        {
            { "claimId", claim.Id.ToString() },
            { "title", claim.Title },
            { "responseDate", claim.ClaimResponse?.ResponseDate.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd") }
        };

        await _notificationDispatcher.SendNotificationAsync(
            userId: claim.User_id.Value,
            type: NotificationType.ClaimResponse,
            title: title,
            body: body,
            relatedEntityId: claim.Id,
            relatedEntityType: "Claim",
            metadata: metadata
        );
    }

    public async Task ExecuteForClaimStatusUpdateAsync(int claimId, string newStatus)
    {
        var claim = await _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new KeyNotFoundException($"No se encontró el reclamo con ID {claimId}");
        }

        if (claim.User_id == null)
        {
            return;
        }

        var statusEmoji = newStatus.ToLower() switch
        {
            "en proceso" or "in progress" => "🔄",
            "pendiente" or "pending" => "⏳",
            "resuelto" or "resolved" => "✅",
            "rechazado" or "rejected" => "❌",
            _ => "📌"
        };

        var title = $"{statusEmoji} Actualización de Reclamo";
        var body = $"Tu reclamo '{claim.Title}' cambió a estado: {newStatus}";

        var metadata = new Dictionary<string, string>
        {
            { "claimId", claim.Id.ToString() },
            { "title", claim.Title },
            { "previousState", claim.State },
            { "newState", newStatus },
            { "updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        await _notificationDispatcher.SendNotificationAsync(
            userId: claim.User_id.Value,
            type: NotificationType.ClaimStatusUpdate,
            title: title,
            body: body,
            relatedEntityId: claim.Id,
            relatedEntityType: "Claim",
            metadata: metadata
        );
    }

    public async Task ExecuteForClaimResolvedAsync(int claimId)
    {
        var claim = await _claimRepository.GetById(claimId);
        if (claim == null)
        {
            throw new KeyNotFoundException($"No se encontró el reclamo con ID {claimId}");
        }

        if (claim.User_id == null)
        {
            return;
        }

        var title = "✅ Reclamo Resuelto";
        var body = $"¡Tu reclamo '{claim.Title}' ha sido marcado como resuelto!";

        var metadata = new Dictionary<string, string>
        {
            { "claimId", claim.Id.ToString() },
            { "title", claim.Title },
            { "resolvedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        await _notificationDispatcher.SendNotificationAsync(
            userId: claim.User_id.Value,
            type: NotificationType.ClaimResolved,
            title: title,
            body: body,
            relatedEntityId: claim.Id,
            relatedEntityType: "Claim",
            metadata: metadata
        );
    }
}
