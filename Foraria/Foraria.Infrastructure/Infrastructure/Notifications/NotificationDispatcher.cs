using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.Extensions.Logging;

namespace Foraria.Infrastructure.Notifications;


public class NotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IFcmPushNotificationService _pushService;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        IFcmPushNotificationService pushService,
        ILogger<NotificationDispatcher> logger)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task<Notification> SendNotificationAsync(
        int userId,
        string type,
        string title,
        string body,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        Dictionary<string, string>? metadata = null)
    {

        var preferences = await _preferenceRepository.GetByUserIdAsync(userId);


        if (preferences == null)
        {
            _logger.LogWarning(
                "Usuario {UserId} no tiene preferencias configuradas, usando valores por defecto",
                userId);

            preferences = new NotificationPreference
            {
                UserId = userId,
                PushEnabled = true,
                ExpenseNotificationsEnabled = true,
                MeetingNotificationsEnabled = true,
                VotingNotificationsEnabled = false,
                ForumNotificationsEnabled = true,
                MaintenanceNotificationsEnabled = true,
                ClaimNotificationsEnabled = true
            };
        }

        if (!IsNotificationTypeEnabled(preferences, type))
        {
            _logger.LogInformation(
                "Notificación de tipo {Type} está deshabilitada para usuario {UserId}",
                type, userId);

            var skippedNotification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Channel = NotificationChannel.Push,
                Status = "Skipped",
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                MetadataJson = metadata != null
                    ? System.Text.Json.JsonSerializer.Serialize(metadata)
                    : null
            };

            return await _notificationRepository.CreateAsync(skippedNotification);
        }

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            Channel = NotificationChannel.Push, 
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            MetadataJson = metadata != null
                ? System.Text.Json.JsonSerializer.Serialize(metadata)
                : null
        };

        await _notificationRepository.CreateAsync(notification);

        if (preferences.PushEnabled && !string.IsNullOrEmpty(preferences.FcmToken))
        {
            try
            {
                var success = await _pushService.SendPushNotificationAsync(
                    preferences.FcmToken,
                    title,
                    body,
                    metadata);

                if (success)
                {
                    await _notificationRepository.MarkAsSentAsync(notification.Id);
                    _logger.LogInformation(
                        "Notificación {NotificationId} enviada a usuario {UserId}",
                        notification.Id, userId);
                }
                else
                {
                    await _notificationRepository.MarkAsFailedAsync(
                        notification.Id,
                        "Error al enviar push notification");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enviar notificación {NotificationId} a usuario {UserId}",
                    notification.Id, userId);

                await _notificationRepository.MarkAsFailedAsync(
                    notification.Id,
                    ex.Message);
            }
        }
        else
        {
            _logger.LogInformation(
                "Push notifications deshabilitado o sin token para usuario {UserId}",
                userId);
        }

        return notification;
    }

    public async Task<List<Notification>> SendBatchNotificationAsync(
        List<int> userIds,
        string type,
        string title,
        string body,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        Dictionary<string, string>? metadata = null)
    {
        var notifications = new List<Notification>();

        foreach (var userId in userIds)
        {
            var notification = await SendNotificationAsync(
                userId, type, title, body,
                relatedEntityId, relatedEntityType, metadata);

            notifications.Add(notification);
        }

        _logger.LogInformation(
            "Envío batch completado: {Count} notificaciones procesadas",
            notifications.Count);

        return notifications;
    }

    public async Task ProcessPendingNotificationsAsync()
    {
        var pendingNotifications = await _notificationRepository.GetPendingNotificationsAsync();

        _logger.LogInformation(
            "Procesando {Count} notificaciones pendientes",
            pendingNotifications.Count());

        foreach (var notification in pendingNotifications)
        {
            try
            {
                var preferences = await _preferenceRepository.GetByUserIdAsync(notification.UserId);

                if (preferences?.PushEnabled == true && !string.IsNullOrEmpty(preferences.FcmToken))
                {
                    var metadata = !string.IsNullOrEmpty(notification.MetadataJson)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                            notification.MetadataJson)
                        : null;

                    var success = await _pushService.SendPushNotificationAsync(
                        preferences.FcmToken,
                        notification.Title,
                        notification.Body,
                        metadata);

                    if (success)
                    {
                        await _notificationRepository.MarkAsSentAsync(notification.Id);
                    }
                    else
                    {
                        await _notificationRepository.MarkAsFailedAsync(
                            notification.Id,
                            "Error en reintento de envío");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error procesando notificación pendiente {NotificationId}",
                    notification.Id);
            }
        }
    }

    private bool IsNotificationTypeEnabled(NotificationPreference preferences, string type)
    {
        return type switch
        {
            var t when t.StartsWith("Expense") => preferences.ExpenseNotificationsEnabled,
            var t when t.StartsWith("Meeting") => preferences.MeetingNotificationsEnabled,
            var t when t.StartsWith("Voting") => preferences.VotingNotificationsEnabled,
            var t when t.StartsWith("Forum") => preferences.ForumNotificationsEnabled,
            var t when t.StartsWith("Maintenance") => preferences.MaintenanceNotificationsEnabled,
            var t when t.StartsWith("Claim") => preferences.ClaimNotificationsEnabled,
            _ => true 
        };
    }
}