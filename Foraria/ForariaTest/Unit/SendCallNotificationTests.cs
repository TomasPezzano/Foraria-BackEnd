using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit;

public class SendCallNotificationTests
{
    private readonly Mock<ICallRepository> _callRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INotificationDispatcher> _notificationDispatcherMock;
    private readonly SendCallNotification _useCase;

    public SendCallNotificationTests()
    {
        _callRepositoryMock = new Mock<ICallRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _notificationDispatcherMock = new Mock<INotificationDispatcher>();

        _useCase = new SendCallNotification(
            _callRepositoryMock.Object,
            _userRepositoryMock.Object,
            _notificationDispatcherMock.Object);
    }

    #region ExecuteForNewCallAsync Tests

    [Fact]
    public async Task ExecuteForNewCallAsync_ValidCall_SendsNotificationToAllUsers()
    {
        // Arrange
        var callId = 1;
        var consortiumId = 10;
        var call = new Call
        {
            Id = callId,
            CreatedByUserId = 100,
            StartedAt = DateTime.Now.AddDays(2),
            Status = "Active",
            ConsortiumId = consortiumId
        };

        var users = new List<User>
        {
            new User { Id = 1, Role = new Role { Description = "Propietario" } },
            new User { Id = 2, Role = new Role { Description = "Inquilino" } },
            new User { Id = 3, Role = new Role { Description = "Consorcio" } },
            new User { Id = 4, Role = new Role { Description = "Administrador" } }
        };

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns(call);

        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(users);

        _notificationDispatcherMock.Setup(d => d.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new List<Notification>());

        // Act
        await _useCase.ExecuteForNewCallAsync(callId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.Is<List<int>>(ids => ids.Count == 4),
            NotificationType.MeetingCreated,
            It.Is<string>(t => t.Contains("Nueva Reunión Virtual")),
            It.IsAny<string>(),
            callId,
            "Call",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteForNewCallAsync_CallNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var callId = 999;

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns((Call?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _useCase.ExecuteForNewCallAsync(callId));

        Assert.Contains($"No se encontró la reunión con ID {callId}", exception.Message);
    }

    [Fact]
    public async Task ExecuteForNewCallAsync_NoUsersInConsortium_DoesNotSendNotification()
    {
        // Arrange
        var callId = 1;
        var consortiumId = 10;
        var call = new Call
        {
            Id = callId,
            CreatedByUserId = 100,
            StartedAt = DateTime.Now.AddDays(2),
            Status = "Active",
            ConsortiumId = consortiumId
        };

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns(call);

        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(new List<User>());

        // Act
        await _useCase.ExecuteForNewCallAsync(callId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.IsAny<List<int>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    #endregion

    #region ExecuteForCallReminderAsync Tests

    [Fact]
    public async Task ExecuteForCallReminderAsync_CallTomorrow_SendsReminder()
    {
        // Arrange
        var callId = 1;
        var consortiumId = 10;
        var tomorrow = DateTime.Now.AddDays(1);

        var call = new Call
        {
            Id = callId,
            CreatedByUserId = 100,
            StartedAt = tomorrow,
            Status = "Active",
            ConsortiumId = consortiumId
        };

        var users = new List<User>
        {
            new User { Id = 1, Role = new Role { Description = "Propietario" } },
            new User { Id = 2, Role = new Role { Description = "Inquilino" } }
        };

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns(call);

        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(users);

        _notificationDispatcherMock.Setup(d => d.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new List<Notification>());

        // Act
        await _useCase.ExecuteForCallReminderAsync(callId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.Is<List<int>>(ids => ids.Count == 2),
            NotificationType.MeetingReminder,
            It.Is<string>(t => t.Contains("Recordatorio")),
            It.IsAny<string>(),
            callId,
            "Call",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteForCallReminderAsync_CallNotTomorrow_DoesNotSendReminder()
    {
        // Arrange
        var callId = 1;
        var consortiumId = 10;
        var nextWeek = DateTime.Now.AddDays(7);

        var call = new Call
        {
            Id = callId,
            CreatedByUserId = 100,
            StartedAt = nextWeek,
            Status = "Active",
            ConsortiumId = consortiumId
        };

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns(call);

        // Act
        await _useCase.ExecuteForCallReminderAsync(callId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.IsAny<List<int>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    #endregion

    #region ExecuteForCallCancelledAsync Tests

    [Fact]
    public async Task ExecuteForCallCancelledAsync_ValidCall_SendsCancellationNotification()
    {
        // Arrange
        var callId = 1;
        var consortiumId = 10;
        var call = new Call
        {
            Id = callId,
            CreatedByUserId = 100,
            StartedAt = DateTime.Now.AddDays(2),
            Status = "Cancelled",
            ConsortiumId = consortiumId
        };

        var users = new List<User>
        {
            new User { Id = 1, Role = new Role { Description = "Propietario" } },
            new User { Id = 2, Role = new Role { Description = "Administrador" } }
        };

        _callRepositoryMock.Setup(r => r.GetById(callId))
            .Returns(call);

        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(users);

        _notificationDispatcherMock.Setup(d => d.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new List<Notification>());

        // Act
        await _useCase.ExecuteForCallCancelledAsync(callId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.Is<List<int>>(ids => ids.Count == 2),
            NotificationType.MeetingCancelled,
            It.Is<string>(t => t.Contains("Cancelada")),
            It.IsAny<string>(),
            callId,
            "Call",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    #endregion
}
