using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit;

public class UpdateUserFirtTimeTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHash> _passwordHashMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly UpdateUserFirstTime _updateUserFirstTime;

    public UpdateUserFirtTimeTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashMock = new Mock<IPasswordHash>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _updateUserFirstTime = new UpdateUserFirstTime(
            _userRepositoryMock.Object,
            _passwordHashMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Update_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdWithRole(It.IsAny<int>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _updateUserFirstTime.Update(
            1, "oldPass", "newPass123!", "John", "Doe", 12345678, null, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Usuario no encontrado", result.Message);
    }

    [Fact]
    public async Task Update_WhenUserAlreadyUpdated_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            RequiresPasswordChange = false  
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRole(1))
            .ReturnsAsync(user);

        // Act
        var result = await _updateUserFirstTime.Update(
            1, "oldPass", "newPass123!", "John", "Doe", 12345678, null, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Este usuario ya completó la actualización de datos", result.Message);
    }

    [Fact]
    public async Task Update_WhenCurrentPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Password = "hashedOldPassword",
            RequiresPasswordChange = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRole(1))
            .ReturnsAsync(user);

        _passwordHashMock
            .Setup(x => x.VerifyPassword("wrongPassword", user.Password))
            .Returns(false);

        // Act
        var result = await _updateUserFirstTime.Update(
            1, "wrongPassword", "newPass123!", "John", "Doe", 12345678, null, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("La contraseña actual es incorrecta", result.Message);
    }

    [Theory]
    [InlineData("short")]           // Too short
    [InlineData("nouppercase123!")]  // No uppercase
    [InlineData("NOLOWERCASE123!")]  // No lowercase
    [InlineData("NoDigits!!!")]      // No digits
    [InlineData("NoSpecial123")]     // No special chars
    public async Task Update_WithWeakPassword_ShouldReturnFailure(string weakPassword)
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Password = "hashedOldPassword",
            RequiresPasswordChange = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRole(1))
            .ReturnsAsync(user);

        _passwordHashMock
            .Setup(x => x.VerifyPassword("oldPassword", user.Password))
            .Returns(true);

        // Act
        var result = await _updateUserFirstTime.Update(
            1, "oldPassword", weakPassword, "John", "Doe", 12345678, null, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("al menos 8 caracteres", result.Message);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnSuccessAndNewTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "hashedOldPassword",
            Role_id = 4,
            RequiresPasswordChange = true,
            Role = new Role { Id = 4, Description = "Inquilino" }
        };

        var newAccessToken = "new.jwt.token";
        var newRefreshToken = "new.refresh.token";
        var newHashedPassword = "newHashedPassword";

        _userRepositoryMock
            .Setup(x => x.GetByIdWithRole(1))
            .ReturnsAsync(user);

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.HashPassword("NewPass123!"))
            .Returns(newHashedPassword);

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(user.Id, user.Mail, user.Role_id, user.Role.Description, false))
            .Returns(newAccessToken);

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns(newRefreshToken);

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _updateUserFirstTime.Update(
            1, "OldPass123!", "NewPass123!", "John", "Doe", 12345678, "/uploads/photo.jpg", "192.168.1.1");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Bienvenido a Foraria", result.Message);
        Assert.Equal(newAccessToken, result.AccessToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);

        // Verify user was updated
        _userRepositoryMock.Verify(x => x.Update(It.Is<User>(u =>
            u.Name == "John" &&
            u.LastName == "Doe" &&
            u.Password == newHashedPassword &&
            u.Dni == 12345678 &&
            u.Photo == "/uploads/photo.jpg" &&
            u.RequiresPasswordChange == false  // ← Key change
        )), Times.Once);

        // Verify new refresh token was saved
        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<RefreshToken>(rt =>
            rt.UserId == 1 &&
            rt.Token == newRefreshToken &&
            rt.CreatedByIp == "192.168.1.1"
        )), Times.Once);
    }
}
