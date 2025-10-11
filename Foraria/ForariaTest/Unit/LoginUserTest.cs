using Xunit;
using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.Extensions.Options;
using ForariaDomain.Aplication.Configuration;

namespace ForariaTest.Unit;

public class LoginUserTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHash> _mockPasswordHash;
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
    private readonly Mock<IRefreshTokenGenerator> _mockRefreshTokenGenerator;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;

    public LoginUserTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPasswordHash = new Mock<IPasswordHash>();
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
        _mockRefreshTokenGenerator = new Mock<IRefreshTokenGenerator>();
        _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();

        _mockJwtSettings.Setup(x => x.Value).Returns(new JwtSettings
        {
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });
    }

    private LoginUser CreateService()
    {
        return new LoginUser(
            _mockUserRepo.Object,
            _mockPasswordHash.Object,
            _mockJwtGenerator.Object,
            _mockRefreshTokenGenerator.Object,
            _mockRefreshTokenRepo.Object,
            _mockJwtSettings.Object
        );
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnSuccessWithBothTokens()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Name = "Juan",
            LastName = "Pérez",
            Password = "$2a$11$hashedpassword",
            Role_id = 1,
            Role = role,
            RequiresPasswordChange = true
        };

        _mockUserRepo.Setup(r => r.GetByEmailWithRole("juan@test.com"))
                     .ReturnsAsync(user);
        _mockPasswordHash.Setup(p => p.VerifyPassword("TempPass123!", user.Password))
                         .Returns(true);
        _mockJwtGenerator.Setup(j => j.Generate(1, "juan@test.com", 1, "Inquilino", true))
                         .Returns("mock.access.token");
        _mockRefreshTokenGenerator.Setup(r => r.Generate())
                                  .Returns("mock.refresh.token");
        _mockRefreshTokenRepo.Setup(r => r.Add(It.IsAny<ForariaDomain.RefreshToken>()))
                             .ReturnsAsync((ForariaDomain.RefreshToken rt) => rt);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "juan@test.com",
            Password = "TempPass123!"
        };

        // Act
        var result = await service.Login(loginDto, "192.168.1.1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.Equal("mock.access.token", result.Token);
        Assert.Equal("mock.refresh.token", result.RefreshToken);
        Assert.True(result.RequiresPasswordChange);
        Assert.NotNull(result.User);
        Assert.Equal(1, result.User.Id);
        Assert.Equal("juan@test.com", result.User.Email);
        Assert.Equal("Juan", result.User.FirstName);
        Assert.Equal("Pérez", result.User.LastName);
        Assert.Equal(1, result.User.RoleId);
        Assert.Equal("Inquilino", result.User.RoleName);

        _mockUserRepo.Verify(r => r.GetByEmailWithRole("juan@test.com"), Times.Once);
        _mockPasswordHash.Verify(p => p.VerifyPassword("TempPass123!", user.Password), Times.Once);
        _mockJwtGenerator.Verify(j => j.Generate(1, "juan@test.com", 1, "Inquilino", true), Times.Once);
        _mockRefreshTokenGenerator.Verify(r => r.Generate(), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.Add(It.Is<ForariaDomain.RefreshToken>(
            rt => rt.UserId == 1 &&
                  rt.Token == "mock.refresh.token" &&
                  rt.CreatedByIp == "192.168.1.1" &&
                  rt.IsRevoked == false
        )), Times.Once);
    }

    [Fact]
    public async Task Login_ValidCredentialsNoPasswordChange_ShouldReturnSuccessWithFalseFlag()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Id = 2,
            Mail = "admin@test.com",
            Name = "María",
            LastName = "González",
            Password = "$2a$11$hashedpassword",
            Role_id = 2,
            Role = role,
            RequiresPasswordChange = false
        };

        _mockUserRepo.Setup(r => r.GetByEmailWithRole("admin@test.com"))
                     .ReturnsAsync(user);
        _mockPasswordHash.Setup(p => p.VerifyPassword("MySecurePass!", user.Password))
                         .Returns(true);
        _mockJwtGenerator.Setup(j => j.Generate(2, "admin@test.com", 2, "Administrador", false))
                         .Returns("mock.access.token2");
        _mockRefreshTokenGenerator.Setup(r => r.Generate())
                                  .Returns("mock.refresh.token2");
        _mockRefreshTokenRepo.Setup(r => r.Add(It.IsAny<ForariaDomain.RefreshToken>()))
                             .ReturnsAsync((ForariaDomain.RefreshToken rt) => rt);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "admin@test.com",
            Password = "MySecurePass!"
        };

        // Act
        var result = await service.Login(loginDto, "10.0.5.20");

        // Assert
        Assert.True(result.Success);
        Assert.False(result.RequiresPasswordChange);
        Assert.Equal("mock.access.token2", result.Token);
        Assert.Equal("mock.refresh.token2", result.RefreshToken);
        Assert.Equal("Administrador", result.User.RoleName);
    }

    [Fact]
    public async Task Login_InvalidEmail_ShouldReturnError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByEmailWithRole("nonexistent@test.com"))
                     .ReturnsAsync((User?)null);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "nonexistent@test.com",
            Password = "SomePassword123!"
        };

        // Act
        var result = await service.Login(loginDto, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.RefreshToken);
        Assert.Null(result.User);

        _mockUserRepo.Verify(r => r.GetByEmailWithRole("nonexistent@test.com"), Times.Once);
        _mockPasswordHash.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtGenerator.Verify(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _mockRefreshTokenRepo.Verify(r => r.Add(It.IsAny<ForariaDomain.RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_InvalidPassword_ShouldReturnError()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "$2a$11$hashedpassword",
            Role_id = 1,
            Role = role
        };

        _mockUserRepo.Setup(r => r.GetByEmailWithRole("juan@test.com"))
                     .ReturnsAsync(user);
        _mockPasswordHash.Setup(p => p.VerifyPassword("WrongPassword!", user.Password))
                         .Returns(false);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "juan@test.com",
            Password = "WrongPassword!"
        };

        // Act
        var result = await service.Login(loginDto, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.RefreshToken);
        Assert.Null(result.User);

        _mockUserRepo.Verify(r => r.GetByEmailWithRole("juan@test.com"), Times.Once);
        _mockPasswordHash.Verify(p => p.VerifyPassword("WrongPassword!", user.Password), Times.Once);
        _mockJwtGenerator.Verify(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _mockRefreshTokenRepo.Verify(r => r.Add(It.IsAny<ForariaDomain.RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_SavesCorrectIpAddress()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "$2a$11$hashedpassword",
            Role_id = 1,
            Role = role,
            RequiresPasswordChange = false
        };

        _mockUserRepo.Setup(r => r.GetByEmailWithRole(It.IsAny<string>()))
                     .ReturnsAsync(user);
        _mockPasswordHash.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(true);
        _mockJwtGenerator.Setup(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                         .Returns("token");
        _mockRefreshTokenGenerator.Setup(r => r.Generate())
                                  .Returns("refresh");
        _mockRefreshTokenRepo.Setup(r => r.Add(It.IsAny<ForariaDomain.RefreshToken>()))
                             .ReturnsAsync((ForariaDomain.RefreshToken rt) => rt);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "juan@test.com",
            Password = "Pass123!"
        };

        // Act
        await service.Login(loginDto, "203.0.113.5");

        // Assert
        _mockRefreshTokenRepo.Verify(r => r.Add(It.Is<ForariaDomain.RefreshToken>(
            rt => rt.CreatedByIp == "203.0.113.5"
        )), Times.Once);
    }
}