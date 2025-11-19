//// Foraria.Tests/Application/UseCase/Notifications/SendPollNotificationTests.cs
//using Foraria.Domain.Repository;
//using ForariaDomain;
//using ForariaDomain.Application.UseCase;
//using ForariaDomain.Repository;
//using Moq;
//using Xunit;

//namespace ForariaTest.Unit;

//public class SendPollNotificationTests
//{
//    private readonly Mock<IPollRepository> _pollRepositoryMock;
//    private readonly Mock<IVoteRepository> _voteRepositoryMock;
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<INotificationDispatcher> _notificationDispatcherMock;
//    private readonly SendPollNotification _useCase;

//    public SendPollNotificationTests()
//    {
//        _pollRepositoryMock = new Mock<IPollRepository>();
//        _voteRepositoryMock = new Mock<IVoteRepository>();
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _notificationDispatcherMock = new Mock<INotificationDispatcher>();

//        _useCase = new SendPollNotification(
//            _pollRepositoryMock.Object,
//            _voteRepositoryMock.Object,
//            _userRepositoryMock.Object,
//            _notificationDispatcherMock.Object
//        );
//    }

//    #region ExecuteForNewPollAsync Tests

//    [Fact]
//    public async Task ExecuteForNewPollAsync_WithValidPoll_SendsNotificationsToProprietariesAndCouncil()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación sobre obras",
//            Description = "Votación para aprobar obras en áreas comunes",
//            EndDate = DateTime.Now.AddDays(7),
//            CategoryPoll = new CategoryPoll { Id = 1, Description = "Obras" }
//        };

//        var users = new List<User>
//    {
//        new User { Id = 1, Name = "Juan Pérez", Role = new Role { Description = "Propietario" } },
//        new User { Id = 2, Name = "María García", Role = new Role { Description = "Propietario" } },
//        new User { Id = 3, Name = "Pedro López", Role = new Role { Description = "Consorcio" } },
//        new User { Id = 4, Name = "Ana Martínez", Role = new Role { Description = "Inquilino" } },
//        new User { Id = 5, Name = "Carlos Rodríguez", Role = new Role { Description = "Administrador" } }
//    };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForNewPollAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                // ✅ CORRECCIÓN: Ahora espera 4 usuarios (1, 2, 3, 5) porque Administrador también recibe
//                It.Is<List<int>>(ids =>
//                    ids.Count == 4 &&
//                    ids.Contains(1) &&
//                    ids.Contains(2) &&
//                    ids.Contains(3) &&
//                    ids.Contains(5)), // Agregado el ID 5 (Administrador)
//                NotificationType.VotingAvailable,
//                "🗳️ Nueva Votación Disponible",
//                It.Is<string>(body => body.Contains("Votación sobre obras")),
//                pollId,
//                "Poll",
//                It.Is<Dictionary<string, string>>(m =>
//                    m.ContainsKey("pollId") &&
//                    m["pollId"] == pollId.ToString())
//            ),
//            Times.Once
//        );
//    }


//    [Fact]
//    public async Task ExecuteForNewPollAsync_WithNonExistentPoll_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var pollId = 999;

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync((Poll?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//            () => _useCase.ExecuteForNewPollAsync(pollId)
//        );

//        Assert.Contains($"No se encontró la votación con ID {pollId}", exception.Message);

//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Never
//        );
//    }

//    [Fact]
//    public async Task ExecuteForNewPollAsync_WithNoEligibleUsers_DoesNotSendNotifications()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación de prueba",
//            EndDate = DateTime.Now.AddDays(7),
//            CategoryPoll = new CategoryPoll { Id = 1, Description = "General" }
//        };

//        var users = new List<User>
//    {
//        new User { Id = 1, Name = "Ana", Role = new Role { Description = "Inquilino" } }
//    };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        // Act
//        await _useCase.ExecuteForNewPollAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Never
//        );
//    }

//    [Fact]
//    public async Task ExecuteForNewPollAsync_IncludesCorrectMetadata()
//    {
//        // Arrange
//        var pollId = 1;
//        var endDate = DateTime.Now.AddDays(7);
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación importante",
//            EndDate = endDate,
//            CategoryPoll = new CategoryPoll { Id = 1, Description = "Mantenimiento" }
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Test", Role = new Role { Description = "Propietario" } }
//        };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForNewPollAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                NotificationType.VotingAvailable,
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                pollId,
//                "Poll",
//                It.Is<Dictionary<string, string>>(m =>
//                    m["pollId"] == pollId.ToString() &&
//                    m["title"] == "Votación importante" &&
//                    m["endDate"] == endDate.ToString("yyyy-MM-dd") &&
//                    m["category"] == "Mantenimiento"
//                )
//            ),
//            Times.Once
//        );
//    }

//    #endregion

//    #region ExecuteForClosingSoonAsync Tests

//    [Fact]
//    public async Task ExecuteForClosingSoonAsync_OnlyNotifiesUsersWhoHaveNotVoted()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación por cerrar",
//            EndDate = DateTime.Now.AddHours(12)
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Usuario 1", Role = new Role { Description = "Propietario" } },
//            new User { Id = 2, Name = "Usuario 2", Role = new Role { Description = "Propietario" } },
//            new User { Id = 3, Name = "Usuario 3", Role = new Role { Description = "Consorcio" } }
//        };

//        var votes = new List<Vote>
//        {
//            new Vote { User_id = 1, Poll_id = pollId }
//        };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _voteRepositoryMock
//            .Setup(r => r.GetVotesByPollIdAsync(pollId))
//            .ReturnsAsync(votes);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForClosingSoonAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.Is<List<int>>(ids =>
//                    ids.Count == 2 &&
//                    ids.Contains(2) &&
//                    ids.Contains(3) &&
//                    !ids.Contains(1)),
//                NotificationType.VotingClosingSoon,
//                "⏰ Votación por Cerrar",
//                It.IsAny<string>(),
//                pollId,
//                "Poll",
//                It.IsAny<Dictionary<string, string>>()
//            ),
//            Times.Once
//        );
//    }

//    [Fact]
//    public async Task ExecuteForClosingSoonAsync_ShowsHoursWhenLessThan24Hours()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación urgente",
//            EndDate = DateTime.Now.AddHours(12)
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Test", Role = new Role { Description = "Propietario" } }
//        };

//        var votes = new List<Vote>();

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _voteRepositoryMock
//            .Setup(r => r.GetVotesByPollIdAsync(pollId))
//            .ReturnsAsync(votes);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForClosingSoonAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                NotificationType.VotingClosingSoon,
//                It.IsAny<string>(),
//                It.Is<string>(body => body.Contains("horas")),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Once
//        );
//    }

//    [Fact]
//    public async Task ExecuteForClosingSoonAsync_ShowsDaysWhenMoreThan24Hours()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación con tiempo",
//            EndDate = DateTime.Now.AddHours(48)
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Test", Role = new Role { Description = "Propietario" } }
//        };

//        var votes = new List<Vote>();

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _voteRepositoryMock
//            .Setup(r => r.GetVotesByPollIdAsync(pollId))
//            .ReturnsAsync(votes);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForClosingSoonAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                NotificationType.VotingClosingSoon,
//                It.IsAny<string>(),
//                It.Is<string>(body => body.Contains("días")),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Once
//        );
//    }

//    [Fact]
//    public async Task ExecuteForClosingSoonAsync_WithAllUsersVoted_DoesNotSendNotifications()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación completa",
//            EndDate = DateTime.Now.AddHours(12)
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Usuario 1", Role = new Role { Description = "Propietario" } },
//            new User { Id = 2, Name = "Usuario 2", Role = new Role { Description = "Propietario" } }
//        };

//        var votes = new List<Vote>
//        {
//            new Vote { User_id = 1, Poll_id = pollId },
//            new Vote { User_id = 2, Poll_id = pollId }
//        };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _voteRepositoryMock
//            .Setup(r => r.GetVotesByPollIdAsync(pollId))
//            .ReturnsAsync(votes);

//        // Act
//        await _useCase.ExecuteForClosingSoonAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Never
//        );
//    }

//    [Fact]
//    public async Task ExecuteForClosingSoonAsync_WithNonExistentPoll_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var pollId = 999;

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync((Poll?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//            () => _useCase.ExecuteForClosingSoonAsync(pollId)
//        );

//        Assert.Contains($"No se encontró la votación con ID {pollId}", exception.Message);
//    }

//    #endregion

//    [Fact]
//    public async Task ExecuteForClosedAsync_WithValidPoll_SendsNotificationsToAllEligibleUsers()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Votación finalizada",
//            EndDate = DateTime.Now.AddDays(-1)
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Name = "Usuario 1", Role = new Role { Description = "Propietario" } },
//            new User { Id = 2, Name = "Usuario 2", Role = new Role { Description = "Consorcio" } },
//            new User { Id = 3, Name = "Usuario 3", Role = new Role { Description = "Inquilino" } }
//        };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        _notificationDispatcherMock
//            .Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForClosedAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.Is<List<int>>(ids =>
//                    ids.Count == 2 &&
//                    ids.Contains(1) &&
//                    ids.Contains(2)),
//                NotificationType.VotingClosed,
//                "✅ Votación Cerrada",
//                It.Is<string>(body =>
//                    body.Contains("Votación finalizada") &&
//                    body.Contains("finalizado")),
//                pollId,
//                "Poll",
//                It.Is<Dictionary<string, string>>(m =>
//                    m["pollId"] == pollId.ToString() &&
//                    m["title"] == "Votación finalizada")
//            ),
//            Times.Once
//        );
//    }

//    [Fact]
//    public async Task ExecuteForClosedAsync_WithNonExistentPoll_ThrowsKeyNotFoundException()
//    {
//        // Arrange
//        var pollId = 999;

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync((Poll?)null);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
//            () => _useCase.ExecuteForClosedAsync(pollId)
//        );

//        Assert.Contains($"No se encontró la votación con ID {pollId}", exception.Message);
//    }

//    [Fact]
//    public async Task ExecuteForClosedAsync_WithNoEligibleUsers_DoesNotSendNotifications()
//    {
//        // Arrange
//        var pollId = 1;
//        var poll = new Poll
//        {
//            Id = pollId,
//            Title = "Test",
//            EndDate = DateTime.Now.AddDays(-1)
//        };

//        var users = new List<User>
//    {
//        new User { Id = 1, Name = "Test", Role = new Role { Description = "Inquilino" } }
//    };

//        _pollRepositoryMock
//            .Setup(r => r.GetByIdAsync(pollId))
//            .ReturnsAsync(poll);

//        _userRepositoryMock
//            .Setup(r => r.GetUsersByConsortiumIdAsync())
//            .ReturnsAsync(users);

//        // Act
//        await _useCase.ExecuteForClosedAsync(pollId);

//        // Assert
//        _notificationDispatcherMock.Verify(
//            d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()
//            ),
//            Times.Never
//        );
//    }
//}