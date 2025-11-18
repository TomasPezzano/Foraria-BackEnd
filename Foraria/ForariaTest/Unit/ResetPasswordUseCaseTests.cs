using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;


namespace Foraria.Test.Application.UseCase;

public class ResetPasswordUseCaseTests
{
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHash> _passwordHashMock;
    private readonly ResetPassword _useCase;

    public ResetPasswordUseCaseTests()
    {
        _passwordResetTokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashMock = new Mock<IPasswordHash>();

        _useCase = new ResetPassword(
            _passwordResetTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _passwordHashMock.Object
        );
    }

    [Fact]
    public async Task Execute_TokenNotFound_ReturnsFailure()
    {
        // Arrange
        var token = "invalid-token";
        var newPassword = "NewSecure123!";
        var ipAddress = "192.168.1.1";

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync((PasswordResetToken?)null);

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El enlace de restablecimiento es inválido o ha expirado", result.Message);

        _userRepositoryMock.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);

        _passwordResetTokenRepositoryMock.Verify(
            x => x.Update(It.IsAny<PasswordResetToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_TokenExpired_ReturnsFailure()
    {
        // Arrange
        var token = "expired-token";
        var newPassword = "NewSecure123!";
        var ipAddress = "192.168.1.1";

        var expiredToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(-1), // Expired 1 minute ago
            User = new User
            {
                Id = 1,
                Mail = "user@example.com",
                Role = new Role { Id = 3, Description = "Propietario" }
            }
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(expiredToken);

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El enlace de restablecimiento es inválido o ha expirado", result.Message);

        _userRepositoryMock.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_TokenAlreadyUsed_ReturnsFailure()
    {
        // Arrange
        var token = "used-token";
        var newPassword = "NewSecure123!";
        var ipAddress = "192.168.1.1";

        var usedToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = true,
            UsedAt = DateTime.Now.AddMinutes(-5),
            ExpiresAt = DateTime.Now.AddMinutes(10), // Still valid time-wise
            User = new User
            {
                Id = 1,
                Mail = "user@example.com",
                Role = new Role { Id = 3, Description = "Propietario" }
            }
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(usedToken);

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El enlace de restablecimiento es inválido o ha expirado", result.Message);
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("nouppercase123!")] // No uppercase
    [InlineData("NOLOWERCASE123!")] // No lowercase
    [InlineData("NoDigitsHere!")] // No digits
    [InlineData("NoSpecial123")] // No special characters
    [InlineData("       ")] // Only whitespace
    [InlineData("")] // Empty
    public async Task Execute_InvalidPassword_ReturnsFailure(string invalidPassword)
    {
        // Arrange
        var token = "valid-token";
        var ipAddress = "192.168.1.1";

        var resetToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            User = new User
            {
                Id = 1,
                Mail = "user@example.com",
                Password = "OldHashedPassword",
                Role = new Role { Id = 3, Description = "Propietario" }
            }
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(resetToken);

        // Act
        var result = await _useCase.Execute(token, invalidPassword, ipAddress);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales", result.Message);

        _userRepositoryMock.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }

    [Theory]
    [InlineData("ValidPass123!")]
    [InlineData("MyP@ssw0rd")]
    [InlineData("Secure#2024")]
    [InlineData("Complex$Pass9")]
    public async Task Execute_ValidPassword_UpdatesUserPassword(string validPassword)
    {
        // Arrange
        var token = "valid-token";
        var ipAddress = "192.168.1.1";
        var hashedPassword = "hashed-new-password";

        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "old-hashed-password",
            RequiresPasswordChange = true,
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        var resetToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            User = user
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(resetToken);

        _passwordHashMock
            .Setup(x => x.Execute(validPassword))
            .Returns(hashedPassword);

        // Act
        var result = await _useCase.Execute(token, validPassword, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Contraseña restablecida exitosamente. Ya puedes iniciar sesión", result.Message);

        Assert.Equal(hashedPassword, user.Password);

        _userRepositoryMock.Verify(
            x => x.Update(It.Is<User>(u => u.Id == 1 && u.Password == hashedPassword)),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ValidRequest_MarksTokenAsUsed()
    {
        // Arrange
        var token = "valid-token";
        var newPassword = "ValidPass123!";
        var ipAddress = "192.168.1.1";

        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "old-password",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        var resetToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            User = user
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(resetToken);

        _passwordHashMock
            .Setup(x => x.Execute(newPassword))
            .Returns("hashed-password");

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.True(result.Success);

        Assert.True(resetToken.IsUsed);
        Assert.NotNull(resetToken.UsedAt);
        Assert.Equal(ipAddress, resetToken.UsedByIp);

        _passwordResetTokenRepositoryMock.Verify(
            x => x.Update(It.Is<PasswordResetToken>(
                t => t.Id == 1 && t.IsUsed && t.UsedByIp == ipAddress)),
            Times.Once);
    }

    [Fact]
    public async Task Execute_ValidRequest_PreservesRequiresPasswordChangeFlag()
    {
        // Arrange
        var token = "valid-token";
        var newPassword = "ValidPass123!";
        var ipAddress = "192.168.1.1";

        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "old-password",
            RequiresPasswordChange = true, // User still needs to change password on first login
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        var resetToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            User = user
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(resetToken);

        _passwordHashMock
            .Setup(x => x.Execute(newPassword))
            .Returns("hashed-password");

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.True(result.Success);

        // RequiresPasswordChange should remain true
        Assert.True(user.RequiresPasswordChange);
    }

    [Fact]
    public async Task Execute_ValidRequest_CallsRepositoriesInCorrectOrder()
    {
        // Arrange
        var token = "valid-token";
        var newPassword = "ValidPass123!";
        var ipAddress = "192.168.1.1";
        var callOrder = new List<string>();

        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "old-password",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        var resetToken = new PasswordResetToken
        {
            Id = 1,
            Token = token,
            UserId = 1,
            IsUsed = false,
            ExpiresAt = DateTime.Now.AddMinutes(10),
            User = user
        };

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetByToken(token))
            .ReturnsAsync(resetToken);

        _passwordHashMock
            .Setup(x => x.Execute(newPassword))
            .Returns("hashed-password");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Callback(() => callOrder.Add("UserUpdate"))
            .Returns(Task.CompletedTask);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.Update(It.IsAny<PasswordResetToken>()))
            .Callback(() => callOrder.Add("TokenUpdate"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Execute(token, newPassword, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, callOrder.Count);
        Assert.Equal("UserUpdate", callOrder[0]); // User should be updated first
        Assert.Equal("TokenUpdate", callOrder[1]); // Then token is marked as used
    }
}