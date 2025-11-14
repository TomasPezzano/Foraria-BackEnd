using Foraria.Application.UseCase;
using Foraria.DTOs;
using Foraria.Infrastructure.Notifications;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IConfigureNotificationPreferences _configurePreferences;
    private readonly IFcmPushNotificationService _fcmService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationRepository notificationRepository,
        IConfigureNotificationPreferences configurePreferences,
        IFcmPushNotificationService fcmService,
        ILogger<NotificationController> logger)
    {
        _notificationRepository = notificationRepository;
        _configurePreferences = configurePreferences;
        _fcmService = fcmService;
        _logger = logger;
    }


    [SwaggerOperation(
    Summary = "Obtiene notificaciones.",
    Description = "Obtiene todas las notificaciones del usuario autenticado"
    )]
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetAuthenticatedUserId();
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return Ok(notifications);
    }

    [SwaggerOperation(
    Summary = "Obtiene notificaciones.",
    Description = "Obtiene notificaciones no leídas del usuario autenticado"
    )]
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var userId = GetAuthenticatedUserId();
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
        return Ok(notifications);
    }


    [SwaggerOperation(
    Summary = "Marca notificaciones.",
    Description = "Marca una notificación como leída"
    )]
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = GetAuthenticatedUserId();

        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null)
        {
            return NotFound("Notificación no encontrada");
        }

        if (notification.UserId != userId)
        {
            return Forbid();
        }

        await _notificationRepository.MarkAsReadAsync(notificationId);
        return Ok(new { message = "Notificación marcada como leída" });
    }


    [SwaggerOperation(
    Summary = "Registra notificaciones.",
    Description = "Registra o actualiza el FCM token del usuario"
    )]
    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterFcmToken([FromBody] RegisterTokenRequest request)
    {
        var userId = GetAuthenticatedUserId();

        if (string.IsNullOrWhiteSpace(request.FcmToken))
        {
            return BadRequest("El FCM token es requerido");
        }

        await _configurePreferences.UpdateFcmTokenAsync(userId, request.FcmToken);

        _logger.LogInformation("FCM token registrado para usuario {UserId}", userId);

        return Ok(new { message = "Token registrado exitosamente" });
    }


    [SwaggerOperation(
    Summary = "Obtiene las preferencias.",
    Description = "Obtiene las preferencias de notificación del usuario autenticado"
    )]
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetAuthenticatedUserId();
        var preferences = await _configurePreferences.GetPreferencesAsync(userId);
        return Ok(preferences);
    }


    [SwaggerOperation(
    Summary = "Actualiza las preferencias.",
    Description = "Actualiza las preferencias de notificación del usuario autenticado"
    )]
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userId = GetAuthenticatedUserId();

        var preferences = new NotificationPreference
        {
            UserId = userId,
            PushEnabled = request.PushEnabled,
            EmailEnabled = request.EmailEnabled,
            SmsEnabled = request.SmsEnabled,
            ExpenseNotificationsEnabled = request.ExpenseNotificationsEnabled,
            MeetingNotificationsEnabled = request.MeetingNotificationsEnabled,
            VotingNotificationsEnabled = request.VotingNotificationsEnabled,
            ForumNotificationsEnabled = request.ForumNotificationsEnabled,
            MaintenanceNotificationsEnabled = request.MaintenanceNotificationsEnabled,
            ClaimNotificationsEnabled = request.ClaimNotificationsEnabled
        };

        var updated = await _configurePreferences.UpdatePreferencesAsync(userId, preferences);

        return Ok(updated);
    }

    /// <summary>
    /// Endpoint de prueba para enviar una notificación push
    /// </summary>
    /// 

    [SwaggerOperation(
    Summary = "Actualiza las preferencias.",
    Description = "Actualiza las preferencias de notificación del usuario autenticado"
    )]
    [HttpPost("test")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    public async Task<IActionResult> TestPush([FromBody] TestPushRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken))
        {
            return BadRequest("FCM token requerido");
        }

        var success = await _fcmService.SendPushNotificationAsync(
            request.FcmToken,
            "🧪 Notificación de Prueba",
            "Si ves esto, las notificaciones push están funcionando correctamente!",
            new Dictionary<string, string>
            {
                { "test", "true" },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        );

        if (success)
        {
            return Ok(new { message = "Notificación de prueba enviada exitosamente" });
        }
        else
        {
            return StatusCode(500, new { message = "Error al enviar notificación de prueba" });
        }
    }

    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        return userId;
    }
}


