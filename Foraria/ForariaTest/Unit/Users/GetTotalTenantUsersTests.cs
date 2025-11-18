using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Users;

public class GetTotalTenantUsersTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly GetTotalTenantUsers _useCase;

    public GetTotalTenantUsersTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _useCase = new GetTotalTenantUsers(_mockUserRepo.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoTenantsExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetTotalUsersByTenantIdAsync())
                     .ReturnsAsync(0);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(0, result);
        _mockUserRepo.Verify(r => r.GetTotalUsersByTenantIdAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTotalTenants_WhenTenantsExist()
    {
        // Arrange
        int expectedCount = 15;
        _mockUserRepo.Setup(r => r.GetTotalUsersByTenantIdAsync())
                     .ReturnsAsync(expectedCount);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockUserRepo.Verify(r => r.GetTotalUsersByTenantIdAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetTotalUsersByTenantIdAsync())
                     .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync());
        _mockUserRepo.Verify(r => r.GetTotalUsersByTenantIdAsync(), Times.Once);
    }
}