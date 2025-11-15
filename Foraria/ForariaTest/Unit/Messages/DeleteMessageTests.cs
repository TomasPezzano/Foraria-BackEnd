using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;

namespace ForariaTest.Unit.Messages;

public class DeleteMessageTests
{
    private readonly Mock<IMessageRepository> _messageRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();

    private DeleteMessage CreateUseCase()
    {
        return new DeleteMessage(_messageRepoMock.Object, _userRepoMock.Object);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenMessageDoesNotExist()
    {
        _messageRepoMock.Setup(x => x.GetById(10))
            .ReturnsAsync((ForariaDomain.Message)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(10, 1)
        );

        Assert.Equal("No se encontró el mensaje con id 10", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        var message = new ForariaDomain.Message { Id = 10, User_id = 3 };

        _messageRepoMock.Setup(x => x.GetById(10))
            .ReturnsAsync(message);

        _userRepoMock.Setup(x => x.GetById(1))
            .ReturnsAsync((User)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(10, 1)
        );

        Assert.Equal("No se encontró el usuario con id 1", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowForbidden_WhenUserHasNoPermission()
    {
        var message = new ForariaDomain.Message { Id = 10, User_id = 5 };
        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "Residente" }
        };

        _messageRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(message);
        _userRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            useCase.ExecuteAsync(10, 1)
        );

        Assert.Equal("No tienes permisos para eliminar este mensaje.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldDoNothing_WhenMessageAlreadyDeleted()
    {
        var message = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 1,
            IsDeleted = true
        };

        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "Consorcio" }
        };

        _messageRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(message);
        _userRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(10, 1);

        // Como ya estaba borrado, no se debe llamar Update
        _messageRepoMock.Verify(x => x.Update(It.IsAny<ForariaDomain.Message>()), Times.Never);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldDeleteMessage_WhenUserIsOwner()
    {
        var message = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 5,
            IsDeleted = false
        };

        var user = new User
        {
            Id = 5,
            Role = new Role { Description = "Residente" }
        };

        _messageRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(message);
        _userRepoMock.Setup(x => x.GetById(5)).ReturnsAsync(user);
        _messageRepoMock.Setup(x => x.Update(It.IsAny<ForariaDomain.Message>()))
            .Returns(Task.CompletedTask);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(10, 5);

        Assert.True(message.IsDeleted);
        Assert.True(message.DeletedAt > DateTime.MinValue);

        _messageRepoMock.Verify(x => x.Update(It.Is<ForariaDomain.Message>(m => m.Id == 10)), Times.Once);
    }


    [Theory]
    [InlineData("Administrador")]
    [InlineData("Consorcio")]
    public async Task ExecuteAsync_ShouldDeleteMessage_WhenAdminOrConsortium(string role)
    {
        var message = new ForariaDomain.Message
        {
            Id = 10,
            User_id = 99,
            IsDeleted = false
        };

        var user = new User
        {
            Id = 2,
            Role = new Role { Description = role }
        };

        _messageRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(message);
        _userRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(10, 2);

        Assert.True(message.IsDeleted);
        Assert.NotEqual(default, message.DeletedAt);

        _messageRepoMock.Verify(x => x.Update(It.Is<ForariaDomain.Message>(m => m.Id == 10)), Times.Once);
    }
}
