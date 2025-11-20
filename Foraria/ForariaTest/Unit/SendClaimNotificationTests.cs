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

public class SendClaimNotificationTests
{

    private readonly Mock<IClaimRepository> _claimRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INotificationDispatcher> _notificationDispatcherMock;
    private readonly SendClaimNotification _useCase;

    public SendClaimNotificationTests()
    {
        _claimRepositoryMock = new Mock<IClaimRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _notificationDispatcherMock = new Mock<INotificationDispatcher>();

        _useCase = new SendClaimNotification(
            _claimRepositoryMock.Object,
            _userRepositoryMock.Object,
            _notificationDispatcherMock.Object);
    }

    #region ExecuteForNewClaimAsync Tests

    [Fact]
    public async Task ExecuteForNewClaimAsync_ValidClaim_SendsNotificationToAllUsers()
    {
        // Arrange
        var claimId = 1;
        var consortiumId = 10;
        var claim = new Claim
        {
            Id = claimId,
            Title = "Problema con ascensor",
            Description = "El ascensor no funciona",
            State = "Pendiente",
            Priority = "Alta",
            Category = "Mantenimiento",
            ConsortiumId = consortiumId,
            User_id = 5,
            CreatedAt = DateTime.Now
        };

        var users = new List<User>
        {
            new User { Id = 1, Role = new Role { Description = "Propietario" } },
            new User { Id = 2, Role = new Role { Description = "Inquilino" } },
            new User { Id = 3, Role = new Role { Description = "Administrador" } }
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

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
        await _useCase.ExecuteForNewClaimAsync(claimId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
            It.Is<List<int>>(ids => ids.Count == 3),
            NotificationType.ClaimCreated,
            It.Is<string>(t => t.Contains("Nuevo Reclamo")),
            It.Is<string>(b => b.Contains("Mantenimiento") && b.Contains("Problema con ascensor")),
            claimId,
            "Claim",
            It.Is<Dictionary<string, string>>(m =>
                m["category"] == "Mantenimiento" &&
                m["priority"] == "Alta")),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteForNewClaimAsync_ClaimNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var claimId = 999;

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync((Claim?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _useCase.ExecuteForNewClaimAsync(claimId));

        Assert.Contains($"No se encontró el reclamo con ID {claimId}", exception.Message);
    }

    #endregion

    #region ExecuteForClaimResponseAsync Tests

    [Fact]
    public async Task ExecuteForClaimResponseAsync_ValidClaim_SendsNotificationToCreator()
    {
        // Arrange
        var claimId = 1;
        var userId = 5;
        var claim = new Claim
        {
            Id = claimId,
            Title = "Problema con ascensor",
            User_id = userId,
            ConsortiumId = 10,
            ClaimResponse = new ClaimResponse
            {
                ResponseDate = DateTime.Now,
                Description = "Estamos trabajando en ello"
            }
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

        _notificationDispatcherMock.Setup(d => d.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new Notification());

        // Act
        await _useCase.ExecuteForClaimResponseAsync(claimId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendNotificationAsync(
            userId,
            NotificationType.ClaimResponse,
            It.Is<string>(t => t.Contains("Respuesta a tu Reclamo")),
            It.IsAny<string>(),
            claimId,
            "Claim",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteForClaimResponseAsync_ClaimWithoutUser_DoesNotSendNotification()
    {
        // Arrange
        var claimId = 1;
        var claim = new Claim
        {
            Id = claimId,
            Title = "Problema con ascensor",
            User_id = null,
            ConsortiumId = 10
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

        // Act
        await _useCase.ExecuteForClaimResponseAsync(claimId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendNotificationAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    #endregion

    #region ExecuteForClaimStatusUpdateAsync Tests

    [Fact]
    public async Task ExecuteForClaimStatusUpdateAsync_ValidStatusChange_SendsNotificationWithCorrectEmoji()
    {
        // Arrange
        var claimId = 1;
        var userId = 5;
        var newStatus = "En Proceso";
        var claim = new Claim
        {
            Id = claimId,
            Title = "Problema con ascensor",
            State = "Pendiente",
            User_id = userId,
            ConsortiumId = 10
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

        _notificationDispatcherMock.Setup(d => d.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new Notification());

        // Act
        await _useCase.ExecuteForClaimStatusUpdateAsync(claimId, newStatus);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendNotificationAsync(
            userId,
            NotificationType.ClaimStatusUpdate,
            It.Is<string>(t => t.Contains("🔄") && t.Contains("Actualización")),
            It.Is<string>(b => b.Contains(newStatus)),
            claimId,
            "Claim",
            It.Is<Dictionary<string, string>>(m =>
                m["previousState"] == "Pendiente" &&
                m["newState"] == newStatus)),
            Times.Once);
    }

    [Theory]
    [InlineData("Resuelto", "✅")]
    [InlineData("Rechazado", "❌")]
    [InlineData("Pendiente", "⏳")]
    public async Task ExecuteForClaimStatusUpdateAsync_DifferentStatuses_UsesCorrectEmoji(
        string status, string expectedEmoji)
    {
        // Arrange
        var claimId = 1;
        var userId = 5;
        var claim = new Claim
        {
            Id = claimId,
            Title = "Test",
            State = "Pendiente",
            User_id = userId,
            ConsortiumId = 10
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

        _notificationDispatcherMock.Setup(d => d.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new Notification());

        // Act
        await _useCase.ExecuteForClaimStatusUpdateAsync(claimId, status);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendNotificationAsync(
            userId,
            NotificationType.ClaimStatusUpdate,
            It.Is<string>(t => t.Contains(expectedEmoji)),
            It.IsAny<string>(),
            claimId,
            "Claim",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    #endregion

    #region ExecuteForClaimResolvedAsync Tests

    [Fact]
    public async Task ExecuteForClaimResolvedAsync_ValidClaim_SendsResolutionNotification()
    {
        // Arrange
        var claimId = 1;
        var userId = 5;
        var claim = new Claim
        {
            Id = claimId,
            Title = "Problema con ascensor",
            State = "Resuelto",
            User_id = userId,
            ConsortiumId = 10
        };

        _claimRepositoryMock.Setup(r => r.GetById(claimId))
            .ReturnsAsync(claim);

        _notificationDispatcherMock.Setup(d => d.SendNotificationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(new Notification());

        // Act
        await _useCase.ExecuteForClaimResolvedAsync(claimId);

        // Assert
        _notificationDispatcherMock.Verify(d => d.SendNotificationAsync(
            userId,
            NotificationType.ClaimResolved,
            It.Is<string>(t => t.Contains("✅") && t.Contains("Resuelto")),
            It.IsAny<string>(),
            claimId,
            "Claim",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    #endregion
}
