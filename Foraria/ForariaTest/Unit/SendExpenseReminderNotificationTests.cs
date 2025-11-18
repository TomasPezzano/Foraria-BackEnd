using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;

namespace Foraria.Test.Application.UseCase;

public class SendExpenseReminderNotificationTests
{
    private readonly Mock<IExpenseRepository> _expenseRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<INotificationDispatcher> _dispatcherMock;
    private readonly SendExpenseReminderNotification _useCase;

    public SendExpenseReminderNotificationTests()
    {
        _expenseRepoMock = new Mock<IExpenseRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _dispatcherMock = new Mock<INotificationDispatcher>();

        _useCase = new SendExpenseReminderNotification(
            _expenseRepoMock.Object,
            _userRepoMock.Object,
            _dispatcherMock.Object
        );
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_ExpenseNotFound_ThrowsKeyNotFoundException()
    {
        _expenseRepoMock
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Expense?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _useCase.ExecuteAsync(99));
    }

    // ─────────────────────────────────────────────────────────────
    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ExpenseNotWithin1Day_NoNotification(int days)
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 10,
            Description = "Expensa Test",
            TotalAmount = 12000,
            ExpirationDate = DateTime.Now.Date.AddDays(days)
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(1))
                        .ReturnsAsync(expense);

        // FIX: siempre devolver lista vacía para evitar ArgumentNullException
        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
                     .ReturnsAsync(new List<User>());

        await _useCase.ExecuteAsync(1);

        _dispatcherMock.Verify(
            x => x.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_NoEligibleUsers_NoNotification()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 10,
            Description = "Expensa Test",
            TotalAmount = 5000,
            ExpirationDate = DateTime.Now.Date
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(1))
                        .ReturnsAsync(expense);

        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
                     .ReturnsAsync(new List<User>()); // sin users

        await _useCase.ExecuteAsync(1);

        _dispatcherMock.Verify(
            x => x.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_FiltersUsersCorrectly()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 10,
            Description = "Prueba",
            TotalAmount = 2000,
            ExpirationDate = DateTime.Now.Date.AddDays(1)
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(1))
                        .ReturnsAsync(expense);

        var users = new List<User>
        {
            new User { Id = 1, Role = new Role { Description = "Propietario" } },
            new User { Id = 2, Role = new Role { Description = "Inquilino" }, HasPermission = true },
            new User { Id = 3, Role = new Role { Description = "Inquilino" }, HasPermission = false },
            new User { Id = 4, Role = new Role { Description = "Administrador" } }
        };

        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
                     .ReturnsAsync(users);

        await _useCase.ExecuteAsync(1);

        _dispatcherMock.Verify(x =>
            x.SendBatchNotificationAsync(
                It.Is<List<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })),
                NotificationType.ExpenseReminder,
                It.IsAny<string>(),
                It.IsAny<string>(),
                1,
                "Expense",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_ExpiringToday_SendsCorrectNotification()
    {
        var expense = new Expense
        {
            Id = 5,
            ConsortiumId = 20,
            Description = "Agua",
            TotalAmount = 15000,
            ExpirationDate = DateTime.Now.Date
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(5))
                        .ReturnsAsync(expense);

        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
                     .ReturnsAsync(new List<User>
                     {
                         new User { Id = 10, Role = new Role { Description = "Propietario" } }
                     });

        await _useCase.ExecuteAsync(5);

        _dispatcherMock.Verify(x =>
            x.SendBatchNotificationAsync(
                It.Is<List<int>>(ids => ids.Contains(10)),
                NotificationType.ExpenseReminder,
                "⏰ Recordatorio de Pago",
                It.Is<string>(body => body.Contains("vence HOY")),
                5,
                "Expense",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_ExpiringTomorrow_SendsCorrectNotification()
    {
        var expense = new Expense
        {
            Id = 7,
            ConsortiumId = 50,
            Description = "Luz",
            TotalAmount = 8000,
            ExpirationDate = DateTime.Now.Date.AddDays(2)
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(7))
            .ReturnsAsync(expense);

        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
            .ReturnsAsync(new List<User>
            {
                new User { Id = 4, Role = new Role { Description = "Propietario" } }
            });

        await _useCase.ExecuteAsync(7);

        _dispatcherMock.Verify(x =>
            x.SendBatchNotificationAsync(
                It.Is<List<int>>(ids => ids.Contains(4)),
                NotificationType.ExpenseReminder,
                "⏰ Recordatorio de Pago",
                It.Is<string>(body => body.Contains("vence MAÑANA")),
                It.Is<int?>(id => id == 7),
                It.Is<string>(t => t == "Expense"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ExecuteAsync_SendsCorrectMetadata()
    {
        var expense = new Expense
        {
            Id = 100,
            ConsortiumId = 200,
            Description = "Test",
            TotalAmount = 9999,
            ExpirationDate = DateTime.Now.Date.AddDays(1)
        };

        _expenseRepoMock.Setup(x => x.GetByIdAsync(100))
            .ReturnsAsync(expense);

        _userRepoMock.Setup(x => x.GetUsersByConsortiumIdAsync())
            .ReturnsAsync(new List<User>
            {
                new User { Id = 1, Role = new Role { Description = "Propietario" } }
            });

        Dictionary<string, string>? capturedMetadata = null;

        _dispatcherMock
            .Setup(x => x.SendBatchNotificationAsync(
                It.IsAny<List<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>>()))
            .Callback((List<int> ids,
                       string type,
                       string title,
                       string body,
                       int? entityId,
                       string? entityType,
                       Dictionary<string, string> metadata)
                => capturedMetadata = metadata)
            .ReturnsAsync(new List<Notification>());

        await _useCase.ExecuteAsync(100);

        Assert.NotNull(capturedMetadata);
        Assert.Equal("100", capturedMetadata["expenseId"]);
        Assert.Equal("9999", capturedMetadata["amount"]);
        Assert.Equal(expense.ExpirationDate.ToString("yyyy-MM-dd"), capturedMetadata["expirationDate"]);
        Assert.Equal("200", capturedMetadata["consortiumId"]);
    }
}
