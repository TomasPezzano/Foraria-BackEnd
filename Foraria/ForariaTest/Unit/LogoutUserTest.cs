using Xunit;
using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit;

public class LogoutUserTests
{
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;

    public LogoutUserTests()
    {
        _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
    }

    private LogoutUser CreateService()
    {
        return new LogoutUser(_mockRefreshTokenRepo.Object);
    }

    [Fact]
    public async Task Logout_ValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshToken = new ForariaDomain.RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid.refresh.token",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            IsRevoked = false
        };

        _mockRefreshTokenRepo.Setup(r => r.GetByToken("valid.refresh.token"))
                             .ReturnsAsync(refreshToken);
        _mockRefreshTokenRepo.Setup(r => r.Update(It.IsAny<ForariaDomain.RefreshToken>()))
                             .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.Logout("valid.refresh.token", "192.168.1.1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Logout successful", result.Message);

        _mockRefreshTokenRepo.Verify(r => r.GetByToken("valid.refresh.token"), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.Update(It.Is<ForariaDomain.RefreshToken>(
            rt => rt.IsRevoked == true &&
                  rt.RevokedAt != null &&
                  rt.RevokedByIp == "192.168.1.1"
        )), Times.Once);
    }

    [Fact]
    public async Task Logout_TokenNotFound_ShouldReturnError()
    {
        // Arrange
        _mockRefreshTokenRepo.Setup(r => r.GetByToken("nonexistent.token"))
                             .ReturnsAsync((ForariaDomain.RefreshToken?)null);

        var service = CreateService();

        // Act
        var result = await service.Logout("nonexistent.token", "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid refresh token", result.Message);

        _mockRefreshTokenRepo.Verify(r => r.GetByToken("nonexistent.token"), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.Update(It.IsAny<ForariaDomain.RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_AlreadyRevokedToken_ShouldStillReturnSuccess()
    {
        // Arrange
        var refreshToken = new ForariaDomain.RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "already.revoked.token",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            IsRevoked = true,
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockRefreshTokenRepo.Setup(r => r.GetByToken("already.revoked.token"))
                             .ReturnsAsync(refreshToken);

        var service = CreateService();

        // Act
        var result = await service.Logout("already.revoked.token", "192.168.1.1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Logout successful", result.Message);

        _mockRefreshTokenRepo.Verify(r => r.Update(It.IsAny<ForariaDomain.RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_SavesCorrectIpAddress()
    {
        // Arrange
        var refreshToken = new ForariaDomain.RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _mockRefreshTokenRepo.Setup(r => r.GetByToken(It.IsAny<string>()))
                             .ReturnsAsync(refreshToken);
        _mockRefreshTokenRepo.Setup(r => r.Update(It.IsAny<ForariaDomain.RefreshToken>()))
                             .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.Logout("token", "203.0.113.20");

        // Assert
        _mockRefreshTokenRepo.Verify(r => r.Update(It.Is<ForariaDomain.RefreshToken>(
            rt => rt.RevokedByIp == "203.0.113.20"
        )), Times.Once);
    }
}