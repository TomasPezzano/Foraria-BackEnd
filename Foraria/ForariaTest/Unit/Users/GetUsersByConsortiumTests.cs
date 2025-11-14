using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;

namespace ForariaTest.Unit.Users;

public class GetUsersByConsortiumTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock;
    private readonly GetUsersByConsortium _useCase;

    public GetUsersByConsortiumTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _consortiumRepositoryMock = new Mock<IConsortiumRepository>();
        _useCase = new GetUsersByConsortium(_userRepositoryMock.Object, _consortiumRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
    {
        // Arrange
        int invalidId = 0;

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(invalidId));

        // Assert
        Assert.StartsWith("El ID del consorcio debe ser mayor a 0.", ex.Message);
        Assert.Equal("consortiumId", ex.ParamName);

        _consortiumRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Never);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenConsortiumDoesNotExist()
    {
        // Arrange
        int consortiumId = 5;

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync((Consortium?)null);

        // Act
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecuteAsync(consortiumId));

        // Assert
        Assert.Equal($"El consorcio con ID {consortiumId} no existe.", ex.Message);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenConsortiumHasNoUsers()
    {
        // Arrange
        int consortiumId = 10;

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync(new Consortium { Id = consortiumId });

        _userRepositoryMock
            .Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(new List<User>()); // Empty

        // Act
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecuteAsync(consortiumId));

        // Assert
        Assert.Equal($"El consorcio con ID {consortiumId} no tiene usuarios asignados", ex.Message);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(consortiumId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnUsers_WhenConsortiumHasUsers()
    {
        // Arrange
        int consortiumId = 3;

        var expectedUsers = new List<User>
        {
            new User { Id = 1, Name = "Juan" },
            new User { Id = 2, Name = "Ana" }
        };

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync(new Consortium { Id = consortiumId });

        _userRepositoryMock
            .Setup(r => r.GetUsersByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _useCase.ExecuteAsync(consortiumId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == 1);
        Assert.Contains(result, u => u.Id == 2);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _userRepositoryMock.Verify(r => r.GetUsersByConsortiumIdAsync(consortiumId), Times.Once);
    }
}
