using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;


namespace Foraria.Test.Application.UseCase;

public class ForgotPasswordUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock;
    private readonly Mock<IPasswordResetTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<ISendEmail> _emailServiceMock;
    private readonly ForgotPassword _useCase;

    public ForgotPasswordUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordResetTokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
        _tokenGeneratorMock = new Mock<IPasswordResetTokenGenerator>();
        _emailServiceMock = new Mock<ISendEmail>();

        _useCase = new ForgotPassword(
            _userRepositoryMock.Object,
            _passwordResetTokenRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task Execute_UserNotFound_ReturnsSuccessWithGenericMessage()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var ipAddress = "192.168.1.1";

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Si el correo existe en nuestro sistema, recibirás un enlace de restablecimiento", result.Message);

        // Verify no token was created
        _passwordResetTokenRepositoryMock.Verify(
            x => x.Add(It.IsAny<PasswordResetToken>()),
            Times.Never);

        // Verify no email was sent
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_UserExists_InvalidatesOldTokens()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "192.168.1.1";
        var user = new User
        {
            Id = 1,
            Mail = email,
            Name = "John",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        var oldToken1 = new PasswordResetToken
        {
            Id = 1,
            UserId = 1,
            Token = "old-token-1",
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        var oldToken2 = new PasswordResetToken
        {
            Id = 2,
            UserId = 1,
            Token = "old-token-2",
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var activeTokens = new List<PasswordResetToken> { oldToken1, oldToken2 };

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync(user);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetActiveTokensByUserId(user.Id))
            .ReturnsAsync(activeTokens);

        _tokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("new-generated-token");

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);

        // Verify both old tokens were invalidated
        _passwordResetTokenRepositoryMock.Verify(
            x => x.Update(It.Is<PasswordResetToken>(t => t.Id == 1 && t.IsUsed)),
            Times.Once);

        _passwordResetTokenRepositoryMock.Verify(
            x => x.Update(It.Is<PasswordResetToken>(t => t.Id == 2 && t.IsUsed)),
            Times.Once);

        Assert.True(oldToken1.IsUsed);
        Assert.True(oldToken2.IsUsed);
        Assert.NotNull(oldToken1.UsedAt);
        Assert.NotNull(oldToken2.UsedAt);
    }

    [Fact]
    public async Task Execute_UserExists_CreatesNewToken()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "192.168.1.1";
        var generatedToken = "secure-random-token-12345";

        var user = new User
        {
            Id = 1,
            Mail = email,
            Name = "John",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync(user);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetActiveTokensByUserId(user.Id))
            .ReturnsAsync(new List<PasswordResetToken>());

        _tokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns(generatedToken);

        PasswordResetToken? capturedToken = null;
        _passwordResetTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(token => capturedToken = token)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);

        _passwordResetTokenRepositoryMock.Verify(
            x => x.Add(It.IsAny<PasswordResetToken>()),
            Times.Once);

        Assert.NotNull(capturedToken);
        Assert.Equal(user.Id, capturedToken.UserId);
        Assert.Equal(generatedToken, capturedToken.Token);
        Assert.False(capturedToken.IsUsed);
        Assert.Equal(ipAddress, capturedToken.CreatedByIp);

        // Verify expiration is approximately 15 minutes
        var expectedExpiration = DateTime.UtcNow.AddMinutes(15);
        Assert.True(Math.Abs((capturedToken.ExpiresAt - expectedExpiration).TotalSeconds) < 5);
    }

    [Fact]
    public async Task Execute_UserExists_SendsEmail()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "192.168.1.1";
        var generatedToken = "secure-token";

        var user = new User
        {
            Id = 1,
            Mail = email,
            Name = "John",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync(user);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetActiveTokensByUserId(user.Id))
            .ReturnsAsync(new List<PasswordResetToken>());

        _tokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns(generatedToken);

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);

        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmail(user.Mail, user.Name, generatedToken),
            Times.Once);
    }

    [Fact]
    public async Task Execute_UserExists_ReturnsGenericSuccessMessage()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "192.168.1.1";

        var user = new User
        {
            Id = 1,
            Mail = email,
            Name = "John",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync(user);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetActiveTokensByUserId(user.Id))
            .ReturnsAsync(new List<PasswordResetToken>());

        _tokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("token");

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Si el correo existe en nuestro sistema, recibirás un enlace de restablecimiento", result.Message);
    }

    [Fact]
    public async Task Execute_UserExistsWithNoActiveTokens_WorksCorrectly()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "192.168.1.1";

        var user = new User
        {
            Id = 1,
            Mail = email,
            Name = "John",
            Role = new Role { Id = 3, Description = "Propietario" }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email))
            .ReturnsAsync(user);

        _passwordResetTokenRepositoryMock
            .Setup(x => x.GetActiveTokensByUserId(user.Id))
            .ReturnsAsync(new List<PasswordResetToken>()); // No active tokens

        _tokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("new-token");

        // Act
        var result = await _useCase.Execute(email, ipAddress);

        // Assert
        Assert.True(result.Success);

        // Verify no updates were attempted (no old tokens to invalidate)
        _passwordResetTokenRepositoryMock.Verify(
            x => x.Update(It.IsAny<PasswordResetToken>()),
            Times.Never);

        // Verify new token was created
        _passwordResetTokenRepositoryMock.Verify(
            x => x.Add(It.IsAny<PasswordResetToken>()),
            Times.Once);
    }
}