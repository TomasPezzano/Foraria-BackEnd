//using Foraria.Domain.Repository;
//using ForariaDomain;
//using ForariaDomain.Application.UseCase;
//using ForariaDomain.Repository;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ForariaTest.Unit;

//public class SendForumNotificationTests
//{
//    private readonly Mock<IThreadRepository> _threadRepositoryMock;
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<INotificationDispatcher> _notificationDispatcherMock;
//    private readonly SendForumNotification _useCase;

//    public SendForumNotificationTests()
//    {
//        _threadRepositoryMock = new Mock<IThreadRepository>();
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _notificationDispatcherMock = new Mock<INotificationDispatcher>();

//        _useCase = new SendForumNotification(
//            _threadRepositoryMock.Object,
//            _userRepositoryMock.Object,
//            _notificationDispatcherMock.Object);
//    }

//    #region ExecuteForNewThreadAsync Tests

//    [Fact]
//    public async Task ExecuteForNewThreadAsync_ValidThread_SendsNotificationToAllExceptCreator()
//    {
//        // Arrange
//        var threadId = 1;
//        var creatorId = 5;
//        var consortiumId = 10;

//        var thread = new ForariaDomain.Thread
//        {
//            Id = threadId,
//            Theme = "Nuevo tema de discusión",
//            Description = "Descripción del tema",
//            UserId = creatorId,
//            ForumId = 1,
//            Forum = new Forum
//            {
//                Id = 1,
//                Category = ForumCategory.General,
//                ConsortiumId = consortiumId
//            },
//            CreatedAt = DateTime.Now,
//            State = "Open"
//        };

//        var users = new List<User>
//        {
//            new User { Id = 1, Role = new Role { Description = "Propietario" } },
//            new User { Id = 2, Role = new Role { Description = "Inquilino" } },
//            new User { Id = creatorId, Role = new Role { Description = "Propietario" } },
//            new User { Id = 4, Role = new Role { Description = "Administrador" } }
//        };

//        _threadRepositoryMock.Setup(r => r.GetById(threadId))
//            .ReturnsAsync(thread);

//        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
//            .ReturnsAsync(users);

//        _notificationDispatcherMock.Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForNewThreadAsync(threadId);

//        // Assert
//        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
//            It.Is<List<int>>(ids => ids.Count == 3 && !ids.Contains(creatorId)),
//            NotificationType.ForumNewThread,
//            It.Is<string>(t => t.Contains("Nuevo Tema")),
//            It.Is<string>(b => b.Contains("Nuevo tema de discusión")),
//            threadId,
//            "Thread",
//            It.Is<Dictionary<string, string>>(m =>
//                m["category"] == ForumCategory.General.ToString())),
//            Times.Once);
//    }


//    [Fact]
//    public async Task ExecuteForNewThreadAsync_OnlyCreatorExists_DoesNotSendNotification()
//    {
//        // Arrange
//        var threadId = 1;
//        var creatorId = 5;
//        var consortiumId = 10;

//        var thread = new ForariaDomain.Thread
//        {
//            Id = threadId,
//            Theme = "Tema solo",
//            UserId = creatorId,
//            ForumId = 1,
//            Forum = new Forum
//            {
//                Id = 1,
//                Category = ForumCategory.General,
//                ConsortiumId = consortiumId
//            }
//        };

//        var users = new List<User>
//        {
//            new User { Id = creatorId, Role = new Role { Description = "Propietario" } }
//        };

//        _threadRepositoryMock.Setup(r => r.GetById(threadId))
//            .ReturnsAsync(thread);

//        _userRepositoryMock.Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
//            .ReturnsAsync(users);

//        // Act
//        await _useCase.ExecuteForNewThreadAsync(threadId);

//        // Assert
//        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
//            It.IsAny<List<int>>(),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<int?>(),
//            It.IsAny<string?>(),
//            It.IsAny<Dictionary<string, string>>()),
//            Times.Never);
//    }

//    #endregion

//    #region ExecuteForNewMessageAsync Tests

//    [Fact]
//    public async Task ExecuteForNewMessageAsync_ValidMessage_NotifiesThreadParticipants()
//    {
//        // Arrange
//        var messageId = 10;
//        var threadId = 1;
//        var threadCreatorId = 5;
//        var messageAuthorId = 7;

//        var thread = new ForariaDomain.Thread
//        {
//            Id = threadId,
//            Theme = "Discusión activa",
//            UserId = threadCreatorId,
//            Messages = new List<Message>
//            {
//                new Message { Id = 1, User_id = threadCreatorId, Thread_id = threadId },
//                new Message { Id = 2, User_id = 6, Thread_id = threadId },
//                new Message { Id = messageId, User_id = messageAuthorId, Thread_id = threadId }
//            }
//        };

//        _threadRepositoryMock.Setup(r => r.GetByIdWithMessagesAsync(messageId))
//            .ReturnsAsync(thread);

//        _notificationDispatcherMock.Setup(d => d.SendBatchNotificationAsync(
//                It.IsAny<List<int>>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<int?>(),
//                It.IsAny<string?>(),
//                It.IsAny<Dictionary<string, string>?>()))
//            .ReturnsAsync(new List<Notification>());

//        // Act
//        await _useCase.ExecuteForNewMessageAsync(messageId);

//        // Assert
//        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
//            It.Is<List<int>>(ids =>
//                ids.Count == 2 &&
//                ids.Contains(threadCreatorId) &&
//                ids.Contains(6) &&
//                !ids.Contains(messageAuthorId)),
//            NotificationType.ForumActivity,
//            It.Is<string>(t => t.Contains("Nueva Respuesta")),
//            It.IsAny<string>(),
//            threadId,
//            "Thread",
//            It.IsAny<Dictionary<string, string>>()),
//            Times.Once);
//    }


//    [Fact]
//    public async Task ExecuteForNewMessageAsync_NoOtherParticipants_DoesNotSendNotification()
//    {
//        // Arrange
//        var messageId = 10;
//        var threadId = 1;
//        var authorId = 5;

//        var thread = new ForariaDomain.Thread
//        {
//            Id = threadId,
//            Theme = "Monólogo",
//            UserId = authorId,
//            Messages = new List<Message>
//            {
//                new Message { Id = messageId, User_id = authorId, Thread_id = threadId }
//            }
//        };

//        _threadRepositoryMock.Setup(r => r.GetByIdWithMessagesAsync(messageId))
//            .ReturnsAsync(thread);

//        // Act
//        await _useCase.ExecuteForNewMessageAsync(messageId);

//        // Assert
//        _notificationDispatcherMock.Verify(d => d.SendBatchNotificationAsync(
//            It.IsAny<List<int>>(),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<int?>(),
//            It.IsAny<string?>(),
//            It.IsAny<Dictionary<string, string>>()),
//            Times.Never);
//    }

//    #endregion
//}
