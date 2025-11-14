using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;

namespace ForariaTest.Unit.Polls;

public class UpdatePollTests
{
    private readonly Mock<IPollRepository> _pollRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdatePoll _useCase;

    public UpdatePollTests()
    {
        _pollRepositoryMock = new Mock<IPollRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _useCase = new UpdatePoll(
            _pollRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenPollDoesNotExist()
    {
        int pollId = 1;
        int userId = 2;

        _pollRepositoryMock
            .Setup(r => r.GetById(pollId))
            .ReturnsAsync((Poll?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(pollId, userId, new Poll()));

        Assert.Equal($"La votación con ID {pollId} no existe.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        int pollId = 1;
        int userId = 2;

        _pollRepositoryMock
            .Setup(r => r.GetById(pollId))
            .ReturnsAsync(new Poll { Id = pollId });

        _userRepositoryMock
            .Setup(r => r.GetById(userId))
            .ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(pollId, userId, new Poll()));

        Assert.Equal($"El usuario con ID {userId} no existe.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserIsNotOwnerOrConsortium()
    {
        int pollId = 1;
        int userId = 10;

        var poll = new Poll
        {
            Id = pollId,
            User_id = 99 
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Vecino" }
        };

        _pollRepositoryMock.Setup(r => r.GetById(pollId)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _useCase.ExecuteAsync(pollId, userId, new Poll()));

        Assert.Equal("No tiene permisos para modificar esta votación.", ex.Message);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Closed")]
    [InlineData("Rejected")]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenPollHasInvalidState(string state)
    {
        int pollId = 1;
        int userId = 1;

        var poll = new Poll
        {
            Id = pollId,
            User_id = userId,
            State = state
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Vecino" }
        };

        _pollRepositoryMock.Setup(r => r.GetById(pollId)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _useCase.ExecuteAsync(pollId, userId, new Poll()));

        Assert.Equal("No se pueden modificar votaciones activas, cerradas o rechazadas.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdatePollFields_WhenDataIsValid()
    {
        int pollId = 1;
        int userId = 1;

        var poll = new Poll
        {
            Id = pollId,
            User_id = userId,
            State = "Draft",
            Title = "Old title",
            Description = "Old description"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Vecino" }
        };

        var updateData = new Poll
        {
            Title = "New title",
            Description = "New description",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            State = "Pending"
        };

        _pollRepositoryMock.Setup(r => r.GetById(pollId)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var result = await _useCase.ExecuteAsync(pollId, userId, updateData);

        Assert.Equal("New title", poll.Title);
        Assert.Equal("New description", poll.Description);
        Assert.Equal(updateData.StartDate, poll.StartDate);
        Assert.Equal(updateData.EndDate, poll.EndDate);
        Assert.Equal("Pending", poll.State);

        _pollRepositoryMock.Verify(r => r.UpdatePoll(poll), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowConsortiumUserToEditPoll()
    {
        int pollId = 1;
        int userId = 500;

        var poll = new Poll
        {
            Id = pollId,
            User_id = 999,
            State = "Draft"
        };

        var user = new User
        {
            Id = userId,
            Role = new Role { Description = "Consorcio" }
        };

        _pollRepositoryMock.Setup(r => r.GetById(pollId)).ReturnsAsync(poll);
        _userRepositoryMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var result = await _useCase.ExecuteAsync(pollId, userId, new Poll { Title = "XX" });

        Assert.Equal("XX", poll.Title);
        _pollRepositoryMock.Verify(r => r.UpdatePoll(poll), Times.Once);
        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
