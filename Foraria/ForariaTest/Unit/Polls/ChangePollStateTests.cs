using Moq;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Polls;
public class ChangePollStateTests
{
    private readonly Mock<IPollRepository> _pollRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ChangePollState CreateUseCase()
    {
        return new ChangePollState(
            _pollRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenPollDoesNotExist()
    {
        _pollRepositoryMock
            .Setup(x => x.GetById(10))
            .ReturnsAsync((Poll)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(10, 1, "Activa")
        );

        Assert.Equal("La votación con ID 10 no existe.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        var poll = new Poll { Id = 10, State = "Pendiente" };

        _pollRepositoryMock.Setup(x => x.GetById(10))
            .ReturnsAsync(poll);

        _userRepositoryMock.Setup(x => x.GetById(5))
            .ReturnsAsync((User)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(10, 5, "Activa")
        );

        Assert.Equal("El usuario con ID 5 no existe.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserIsNotConsorcio()
    {
        var poll = new Poll { Id = 10, State = "Pendiente" };
        var user = new User
        {
            Id = 3,
            Role = new Role { Description = "Admin" }
        };

        _pollRepositoryMock.Setup(x => x.GetById(10)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(x => x.GetById(3)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            useCase.ExecuteAsync(10, 3, "Activa")
        );

        Assert.Equal("Solo los usuarios con rol Consorcio pueden cambiar el estado de votaciones.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenPollIsNotPending()
    {
        var poll = new Poll { Id = 10, State = "Activa" };
        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "Consorcio" }
        };

        _pollRepositoryMock.Setup(x => x.GetById(10)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(x => x.GetById(1)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(10, 1, "Activa")
        );

        Assert.Equal("Solo pueden modificarse votaciones en estado Pendiente.", ex.Message);
    }


    [Theory]
    [InlineData("Finalizada")]
    [InlineData("")]
    [InlineData("cancelada")]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenNewStateIsInvalid(string newState)
    {
        var poll = new Poll { Id = 10, State = "Pendiente" };
        var user = new User
        {
            Id = 1,
            Role = new Role { Description = "Consorcio" }
        };

        _pollRepositoryMock.Setup(x => x.GetById(10)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(x => x.GetById(1)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(10, 1, newState)
        );

        Assert.Equal("El nuevo estado debe ser 'Activa' o 'Rechazada'.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldUpdatePoll_WhenValid()
    {
        var poll = new Poll
        {
            Id = 10,
            State = "Pendiente"
        };

        var user = new User
        {
            Id = 7,
            Role = new Role { Description = "Consorcio" }
        };

        _pollRepositoryMock.Setup(x => x.GetById(10)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(x => x.GetById(7)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(10, 7, "Activa");

        Assert.NotNull(result);
        Assert.Equal("Activa", result.State);
        Assert.Equal(7, result.ApprovedByUserId);
        Assert.True(result.ApprovedAt > DateTime.MinValue);

        _pollRepositoryMock.Verify(
            x => x.UpdatePoll(It.Is<Poll>(p => p.Id == 10 && p.State == "Activa")),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once
        );
    }
}
