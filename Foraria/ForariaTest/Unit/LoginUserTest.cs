using Xunit;
using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace ForariaTest.Unit;

public class LoginUserTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHash> _mockPasswordHash;
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;

    public LoginUserTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPasswordHash = new Mock<IPasswordHash>();
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
    }

    private LoginUser CreateService()
    {
        return new LoginUser(
            _mockUserRepo.Object,
            _mockPasswordHash.Object,
            _mockJwtGenerator.Object
        );
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnSuccessWithToken()
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
                         .Returns("mock.jwt.token");

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "juan@test.com",
            Password = "TempPass123!"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.Equal("mock.jwt.token", result.Token);
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
                         .Returns("mock.jwt.token2");

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "admin@test.com",
            Password = "MySecurePass!"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.RequiresPasswordChange);
        Assert.Equal("mock.jwt.token2", result.Token);
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
        var result = await service.Login(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.User);

        _mockUserRepo.Verify(r => r.GetByEmailWithRole("nonexistent@test.com"), Times.Once);
        _mockPasswordHash.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtGenerator.Verify(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
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
        var result = await service.Login(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.User);

        _mockUserRepo.Verify(r => r.GetByEmailWithRole("juan@test.com"), Times.Once);
        _mockPasswordHash.Verify(p => p.VerifyPassword("WrongPassword!", user.Password), Times.Once);
        _mockJwtGenerator.Verify(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Login_EmptyEmail_ShouldReturnError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByEmailWithRole(""))
                     .ReturnsAsync((User?)null);

        var service = CreateService();
        var loginDto = new LoginRequestDto
        {
            Email = "",
            Password = "SomePassword123!"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
    }
}