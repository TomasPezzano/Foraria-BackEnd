using Foraria.Infrastructure.Infrastructure.Notifications;
using ForariaDomain;
using ForariaDomain.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit;
public class NotificationDispatcherTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<INotificationPreferenceRepository> _preferenceRepositoryMock;
    private readonly Mock<IFcmPushNotificationService> _fcmServiceMock;
    private readonly Mock<ILogger<NotificationDispatcher>> _loggerMock;
    private readonly NotificationDispatcher _dispatcher;

    public NotificationDispatcherTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _preferenceRepositoryMock = new Mock<INotificationPreferenceRepository>();
        _fcmServiceMock = new Mock<IFcmPushNotificationService>();
        _loggerMock = new Mock<ILogger<NotificationDispatcher>>();

        _dispatcher = new NotificationDispatcher(
            _notificationRepositoryMock.Object,
            _preferenceRepositoryMock.Object,
            _fcmServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task SendNotificationAsync_WithValidPreferences_SendsPushSuccessfully()
    {
        // Arrange
        var userId = 1;
        var fcmToken = "test-fcm-token-123";

        var preferences = new NotificationPreference
        {
            UserId = userId,
            PushEnabled = true,
            ExpenseNotificationsEnabled = true,
            FcmToken = fcmToken
        };

        // ✅ CRÍTICO: Usar callback para capturar y setear el ID
        Notification? capturedNotification = null;
        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                n.Id = 1; // Simular que EF asigna el ID
                capturedNotification = n;
            })
            .ReturnsAsync((Notification n) => n);

        _preferenceRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _notificationRepositoryMock
            .Setup(x => x.MarkAsSentAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _fcmServiceMock
            .Setup(x => x.SendPushNotificationAsync(
                fcmToken,
                "Test",
                "Test body",
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _dispatcher.SendNotificationAsync(
            userId,
            NotificationType.ExpenseReminder,
            "Test",
            "Test body"
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedNotification);
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);
        _fcmServiceMock.Verify(x => x.SendPushNotificationAsync(
            fcmToken,
            "Test",
            "Test body",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
        _notificationRepositoryMock.Verify(x => x.MarkAsSentAsync(1), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_WithDisabledNotificationType_CreatesSkippedNotification()
    {
        // Arrange
        var userId = 1;

        var preferences = new NotificationPreference
        {
            UserId = userId,
            PushEnabled = true,
            ExpenseNotificationsEnabled = false, // Deshabilitado
            FcmToken = "test-token"
        };

        var skippedNotification = new Notification
        {
            Id = 1,
            UserId = userId,
            Status = "Skipped"
        };

        _preferenceRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(skippedNotification);

        // Act
        var result = await _dispatcher.SendNotificationAsync(
            userId,
            NotificationType.ExpenseReminder,
            "Test",
            "Test body"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Skipped", result.Status);
        _fcmServiceMock.Verify(
            x => x.SendPushNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendNotificationAsync_WithNullPreferences_UsesDefaults()
    {
        // Arrange
        var userId = 1;

        _preferenceRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((NotificationPreference?)null);

        var notification = new Notification
        {
            Id = 1,
            UserId = userId,
            Status = NotificationStatus.Pending
        };

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _dispatcher.SendNotificationAsync(
            userId,
            NotificationType.ExpenseReminder,
            "Test",
            "Test body"
        );

        // Assert
        Assert.NotNull(result);
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);
        // No debe enviar push porque no hay token
        _fcmServiceMock.Verify(
            x => x.SendPushNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendNotificationAsync_WhenFcmFails_MarksAsFailed()
    {
        // Arrange
        var userId = 1;
        var fcmToken = "test-token";

        var preferences = new NotificationPreference
        {
            UserId = userId,
            PushEnabled = true,
            ExpenseNotificationsEnabled = true,
            FcmToken = fcmToken
        };

        // ✅ CRÍTICO: Usar callback para capturar y setear el ID
        Notification? capturedNotification = null;
        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                n.Id = 1; // Simular que EF asigna el ID
                capturedNotification = n;
            })
            .ReturnsAsync((Notification n) => n);

        _preferenceRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(preferences);

        _notificationRepositoryMock
            .Setup(x => x.MarkAsFailedAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _fcmServiceMock
            .Setup(x => x.SendPushNotificationAsync(
                fcmToken,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(false); // Falla el envío

        // Act
        var result = await _dispatcher.SendNotificationAsync(
            userId,
            NotificationType.ExpenseReminder,
            "Test",
            "Test body"
        );

        // Assert
        Assert.NotNull(capturedNotification);
        _notificationRepositoryMock.Verify(
            x => x.MarkAsFailedAsync(1, "Error al enviar push notification"),
            Times.Once);
    }

    [Fact]
    public async Task SendBatchNotificationAsync_SendsToMultipleUsers()
    {
        // Arrange
        var userIds = new List<int> { 1, 2, 3 };

        // ✅ Setup genérico que funciona para todos los userId
        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                // Simular que EF asigna el ID basado en el UserId
                n.Id = n.UserId;
            })
            .ReturnsAsync((Notification n) => n);

        _notificationRepositoryMock
            .Setup(x => x.MarkAsSentAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        foreach (var userId in userIds)
        {
            var preferences = new NotificationPreference
            {
                UserId = userId,
                PushEnabled = true,
                ExpenseNotificationsEnabled = true,
                FcmToken = $"token-{userId}"
            };

            _preferenceRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(preferences);

            _fcmServiceMock
                .Setup(x => x.SendPushNotificationAsync(
                    $"token-{userId}",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);
        }

        // Act
        var results = await _dispatcher.SendBatchNotificationAsync(
            userIds,
            NotificationType.ExpenseReminder,
            "Test",
            "Test body"
        );

        // Assert
        Assert.Equal(3, results.Count);
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Exactly(3));
        _fcmServiceMock.Verify(
            x => x.SendPushNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Exactly(3));
        _notificationRepositoryMock.Verify(x => x.MarkAsSentAsync(It.IsAny<int>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_RetriesFailedNotifications()
    {
        // Arrange
        var pendingNotifications = new List<Notification>
        {
            new Notification
            {
                Id = 1,
                UserId = 1,
                Title = "Test",
                Body = "Test body",
                Status = NotificationStatus.Pending
            }
        };

        var preferences = new NotificationPreference
        {
            UserId = 1,
            PushEnabled = true,
            FcmToken = "test-token"
        };

        _notificationRepositoryMock
            .Setup(x => x.GetPendingNotificationsAsync())
            .ReturnsAsync(pendingNotifications);

        _preferenceRepositoryMock
            .Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(preferences);

        _notificationRepositoryMock
            .Setup(x => x.MarkAsSentAsync(1))
            .Returns(Task.CompletedTask);

        _fcmServiceMock
            .Setup(x => x.SendPushNotificationAsync(
                "test-token",
                "Test",
                "Test body",
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(true);

        // Act
        await _dispatcher.ProcessPendingNotificationsAsync();

        // Assert
        _notificationRepositoryMock.Verify(x => x.GetPendingNotificationsAsync(), Times.Once);
        _fcmServiceMock.Verify(
            x => x.SendPushNotificationAsync(
                "test-token",
                "Test",
                "Test body",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
        _notificationRepositoryMock.Verify(x => x.MarkAsSentAsync(1), Times.Once);
    }
}