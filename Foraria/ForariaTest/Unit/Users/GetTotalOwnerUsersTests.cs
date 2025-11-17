using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Users;

public class GetTotalOwnerUsersTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly GetTotalOwnerUsers _useCase;

    public GetTotalOwnerUsersTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _useCase = new GetTotalOwnerUsers(_mockUserRepo.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoOwnersExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetTotalOwnerUsersAsync())
                     .ReturnsAsync(0);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(0, result);
        _mockUserRepo.Verify(r => r.GetTotalOwnerUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTotalOwners_WhenOwnersExist()
    {
        // Arrange
        int expectedCount = 25;
        _mockUserRepo.Setup(r => r.GetTotalOwnerUsersAsync())
                     .ReturnsAsync(expectedCount);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockUserRepo.Verify(r => r.GetTotalOwnerUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetTotalOwnerUsersAsync())
                     .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync());
        _mockUserRepo.Verify(r => r.GetTotalOwnerUsersAsync(), Times.Once);
    }
}