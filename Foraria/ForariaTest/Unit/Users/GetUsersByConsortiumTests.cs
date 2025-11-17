using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Users;

public class GetUsersByConsortiumTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUsersByConsortium _useCase;

    public GetUsersByConsortiumTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _useCase = new GetUsersByConsortium(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenConsortiumHasNoUsers()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetUsersByConsortiumIdAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUsers_WhenConsortiumHasUsers()
    {
        // Arrange
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Name = "Juan" },
            new User { Id = 2, Name = "Ana" }
        };

        _userRepositoryMock
            .Setup(r => r.GetUsersByConsortiumIdAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == 1);
        Assert.Contains(result, u => u.Id == 2);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetUsersByConsortiumIdAsync())
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync());

        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(), Times.Once);
    }
}