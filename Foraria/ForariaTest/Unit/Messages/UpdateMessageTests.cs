using FluentAssertions;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;

namespace ForariaTest.Unit.Messages;

public class UpdateMessageTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r => r.GetById(It.IsAny<int>()))
                    .ReturnsAsync((User?)null);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 1 };

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(editedMessage, 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("No se encontró el usuario con id 1");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenMessageDoesNotExist()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User { Id = 1, Role = new Role { Description = "admin" } };

        mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(1)).ReturnsAsync((ForariaDomain.Message?)null);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 1 };

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(editedMessage, 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("No se encontró el mensaje con id 1");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenMessageIsDeleted()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User { Id = 1, Role = new Role { Description = "admin" } };
        var msg = new ForariaDomain.Message { Id = 1, IsDeleted = true };

        mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(1)).ReturnsAsync(msg);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 1 };

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(editedMessage, 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede editar un mensaje eliminado.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateContent_WhenOwnerEditsWithin15Minutes()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "residente" }
        };

        var msg = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 1,
            Content = "Viejo",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsDeleted = false
        };

        mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 10, Content = "Nuevo contenido" };

        // Act
        var result = await useCase.ExecuteAsync(editedMessage, 1);

        // Assert
        result.Content.Should().Be("Nuevo contenido");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        mockMsgRepo.Verify(r => r.Update(It.IsAny<ForariaDomain.Message>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowForbidden_WhenOwnerEditsAfter15Minutes()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "residente" }
        };

        var msg = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 1,
            Content = "Viejo",
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            IsDeleted = false
        };

        mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 10, Content = "Tarde" };

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(editedMessage, 1);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Solo puedes editar tu mensaje dentro de los primeros 15 minutos.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowForbidden_WhenUserIsNotOwnerOrAdmin()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User
        {
            Id = 2,
            Role = new Role { Description = "residente" }
        };

        var msg = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 1,
            Content = "Mensaje original",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsDeleted = false
        };

        mockUserRepo.Setup(r => r.GetById(2)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 10, Content = "Intento no autorizado" };

        // Act
        Func<Task> act = async () => await useCase.ExecuteAsync(editedMessage, 2);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("No tienes permisos para editar este mensaje.");

        mockMsgRepo.Verify(r => r.Update(It.IsAny<ForariaDomain.Message>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowAdminToEditAnyMessage()
    {
        // Arrange
        var mockMsgRepo = new Mock<IMessageRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        var user = new User
        {
            Id = 99,
            Role = new Role { Description = "admin" }
        };

        var msg = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 1,
            Content = "Original",
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            IsDeleted = false
        };

        mockUserRepo.Setup(r => r.GetById(99)).ReturnsAsync(user);
        mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

        var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
        var editedMessage = new ForariaDomain.Message { Id = 10, Content = "Actualizado por admin" };

        // Act
        var result = await useCase.ExecuteAsync(editedMessage, 99);

        // Assert
        result.Content.Should().Be("Actualizado por admin");
        mockMsgRepo.Verify(r => r.Update(It.IsAny<ForariaDomain.Message>()), Times.Once);
    }
}
