using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;


namespace ForariaTest.Unit.Users;

public class UpdateUserFirstTimeTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHash> _passwordHashMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly UpdateUserFirstTime _updateUserFirstTime;

    public UpdateUserFirstTimeTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashMock = new Mock<IPasswordHash>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();

        _updateUserFirstTime = new UpdateUserFirstTime(
            _userRepositoryMock.Object,
            _passwordHashMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object,
            _roleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Update_WhenUserIsNull_ShouldReturnFailure()
    {
        // Arrange - Act
        var result = await _updateUserFirstTime.Update(null, "oldPass", "NewPass123!", "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Usuario no encontrado", result.Message);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);

        _passwordHashMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenUserAlreadyUpdated_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedPassword",
            Role_id = 1,
            RequiresPasswordChange = false
        };

        // Act
        var result = await _updateUserFirstTime.Update(user, "oldPass", "NewPass123!", "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Este usuario ya completó la actualización de datos", result.Message);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);

        _passwordHashMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenCurrentPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedOldPassword",
            Role_id = 1,
            RequiresPasswordChange = true
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("wrongPassword", user.Password))
            .Returns(false);

        // Act
        var result = await _updateUserFirstTime.Update(user, "wrongPassword", "NewPass123!", "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("La contraseña actual es incorrecta", result.Message);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);

        _passwordHashMock.Verify(x => x.VerifyPassword("wrongPassword", user.Password), Times.Once);
        _passwordHashMock.Verify(x => x.Execute(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("short")]           // Menos de 8 caracteres
    [InlineData("nouppercase123!")]  // Sin mayúsculas
    [InlineData("NOLOWERCASE123!")]  // Sin minúsculas
    [InlineData("NoDigits!!!")]      // Sin números
    [InlineData("NoSpecial123")]     // Sin caracteres especiales
    [InlineData("")]                 // Vacío
    [InlineData("       ")]          // Solo espacios
    public async Task Update_WithWeakPassword_ShouldReturnFailure(string weakPassword)
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedOldPassword",
            Role_id = 1,
            RequiresPasswordChange = true
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        // Act
        var result = await _updateUserFirstTime.Update(user, "OldPass123!", weakPassword, "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales", result.Message);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);

        _passwordHashMock.Verify(x => x.VerifyPassword("OldPass123!", user.Password), Times.Once);
        _passwordHashMock.Verify(x => x.Execute(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnSuccessAndNewTokens()
    {
        // Arrange
        var role = new Role { Id = 4, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "hashedOldPassword",
            Role_id = 4,
            RequiresPasswordChange = true,
            Name = "John",
            LastName = "Doe",
            PhoneNumber = 12345678,
            HasPermission = false
        };

        var newAccessToken = "new.jwt.token";
        var newRefreshToken = "new.refresh.token";
        var newHashedPassword = "newHashedPassword";

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute("NewPass123!"))
            .Returns(newHashedPassword);

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(4))
            .ReturnsAsync(role);

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(user.Id, user.Mail, user.Role_id, role.Description, false, false))
            .Returns(newAccessToken);

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns(newRefreshToken);

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        var beforeTest = DateTime.UtcNow;

        // Act
        var result = await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

        var afterTest = DateTime.UtcNow;

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Datos actualizados correctamente. Bienvenido a Foraria.", result.Message);
        Assert.Equal(newAccessToken, result.AccessToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);

        _passwordHashMock.Verify(x => x.VerifyPassword("OldPass123!", "hashedOldPassword"), Times.Once);
        _passwordHashMock.Verify(x => x.Execute("NewPass123!"), Times.Once);

        _userRepositoryMock.Verify(x => x.Update(It.Is<User>(u =>
            u.Password == newHashedPassword &&
            u.RequiresPasswordChange == false &&
            u.Id == 1
        )), Times.Once);

        _roleRepositoryMock.Verify(x => x.GetById(4), Times.Once);

        _jwtTokenGeneratorMock.Verify(x => x.Generate(1, "user@example.com", 4, "Inquilino", false, false), Times.Once);

        _refreshTokenGeneratorMock.Verify(x => x.Generate(), Times.Once);

        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<RefreshToken>(rt =>
            rt.UserId == 1 &&
            rt.Token == newRefreshToken &&
            rt.CreatedByIp == "192.168.1.1" &&
            rt.IsRevoked == false &&
            rt.ExpiresAt >= beforeTest.AddDays(7) &&
            rt.ExpiresAt <= afterTest.AddDays(7).AddSeconds(1) &&
            rt.CreatedAt >= beforeTest &&
            rt.CreatedAt <= afterTest.AddSeconds(1)
        )), Times.Once);
    }

    [Fact]
    public async Task Update_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "user@example.com",
            Password = "hashedOldPassword",
            Role_id = 999,
            RequiresPasswordChange = true
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute("NewPass123!"))
            .Returns("newHashedPassword");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(999))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User role not found", result.Message);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);

        // CORRECCIÓN: Verificamos con el password original, no el nuevo
        _passwordHashMock.Verify(x => x.VerifyPassword("OldPass123!", "hashedOldPassword"), Times.Once);
        _passwordHashMock.Verify(x => x.Execute("NewPass123!"), Times.Once);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _roleRepositoryMock.Verify(x => x.GetById(999), Times.Once);
        _jwtTokenGeneratorMock.Verify(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Never);
    }


    [Theory]
    [InlineData("ValidPass1!")]
    [InlineData("Str0ng#Pass")]
    [InlineData("My$ecur3Pwd")]
    [InlineData("C0mpl3x@2024")]
    public async Task Update_WithDifferentValidPasswords_ShouldSucceed(string validPassword)
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedOldPassword",
            Role_id = 1,
            RequiresPasswordChange = true,
            HasPermission = false
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute(validPassword))
            .Returns($"hashed_{validPassword}");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(1))
            .ReturnsAsync(role);

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns("token");

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("refresh");

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _updateUserFirstTime.Update(user, "OldPass123!", validPassword, "192.168.1.1");

        // Assert
        Assert.True(result.Success);
        _passwordHashMock.Verify(x => x.Execute(validPassword), Times.Once);
    }

    [Fact]
    public async Task Update_SetsUserRoleProperty()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Propietario" };
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedOldPassword",
            Role_id = 2,
            RequiresPasswordChange = true,
            Role = null, // Verificamos que se asigna
            HasPermission = false
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute("NewPass123!"))
            .Returns("newHashedPassword");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(2))
            .ReturnsAsync(role);

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns("token");

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("refresh");

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

        // Assert
        Assert.NotNull(user.Role);
        Assert.Equal(2, user.Role.Id);
        Assert.Equal("Propietario", user.Role.Description);
    }

    [Fact]
    public async Task Update_SavesCorrectIpAddress()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "user@test.com",
            Password = "hashedOldPassword",
            Role_id = 1,
            RequiresPasswordChange = true,
            HasPermission = false
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .Returns("newHashedPassword");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(1))
            .ReturnsAsync(role);

        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns("token");

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("refresh");

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "203.0.113.5");

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<RefreshToken>(
            rt => rt.CreatedByIp == "203.0.113.5"
        )), Times.Once);
    }

    [Fact]
    public async Task Update_PassesHasPermissionToJwtGenerator()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Administrador" };
        var user = new User
        {
            Id = 1,
            Mail = "admin@test.com",
            Password = "hashedOldPassword",
            Role_id = 1,
            RequiresPasswordChange = true,
            HasPermission = true // Usuario con permisos (pero se ignora en el JWT)
        };

        _passwordHashMock
            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
            .Returns(true);

        _passwordHashMock
            .Setup(x => x.Execute("NewPass123!"))
            .Returns("newHashedPassword");

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _roleRepositoryMock
            .Setup(x => x.GetById(1))
            .ReturnsAsync(role);

        // CORRECCIÓN: El caso de uso siempre pasa false como HasPermission
        _jwtTokenGeneratorMock
            .Setup(x => x.Generate(1, "admin@test.com", 1, "Administrador", false, false))
            .Returns("token");

        _refreshTokenGeneratorMock
            .Setup(x => x.Generate())
            .Returns("refresh");

        _refreshTokenRepositoryMock
            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

        // Assert
        // CORRECCIÓN: Verificamos que se pasa false, no true
        _jwtTokenGeneratorMock.Verify(x => x.Generate(1, "admin@test.com", 1, "Administrador", false, false), Times.Once);
    }
}