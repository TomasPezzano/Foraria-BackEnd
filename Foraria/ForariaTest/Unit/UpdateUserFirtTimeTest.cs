//using Foraria.Application.UseCase;
//using Foraria.Domain.Repository;
//using ForariaDomain;
//using Moq;
//using Xunit;
//using System.Threading.Tasks;

//namespace ForariaTest.Unit;

//public class UpdateUserFirstTimeTest
//{
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<IPasswordHash> _passwordHashMock;
//    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
//    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
//    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
//    private readonly Mock<IRoleRepository> _roleRepositoryMock;
//    private readonly UpdateUserFirstTime _updateUserFirstTime;

//    public UpdateUserFirstTimeTest()
//    {
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _passwordHashMock = new Mock<IPasswordHash>();
//        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
//        _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
//        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
//        _roleRepositoryMock = new Mock<IRoleRepository>();

//        _updateUserFirstTime = new UpdateUserFirstTime(
//            _userRepositoryMock.Object,
//            _passwordHashMock.Object,
//            _jwtTokenGeneratorMock.Object,
//            _refreshTokenGeneratorMock.Object,
//            _refreshTokenRepositoryMock.Object,
//            _roleRepositoryMock.Object
//        );
//    }

//    [Fact]
//    public async Task Update_WhenUserIsNull_ShouldReturnFailure()
//    {
//        // Arrange - Act
//        var result = await _updateUserFirstTime.Update(null, "oldPass", "newPass123!", "192.168.1.1");

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal("Usuario no encontrado", result.Message);
//    }

//    [Fact]
//    public async Task Update_WhenUserAlreadyUpdated_ShouldReturnFailure()
//    {
//        // Arrange
//        var user = new User
//        {
//            Id = 1,
//            RequiresPasswordChange = false
//        };

//        // Act
//        var result = await _updateUserFirstTime.Update(user, "oldPass", "newPass123!", "192.168.1.1");

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal("Este usuario ya completó la actualización de datos", result.Message);
//    }

//    [Fact]
//    public async Task Update_WhenCurrentPasswordIncorrect_ShouldReturnFailure()
//    {
//        // Arrange
//        var user = new User
//        {
//            Id = 1,
//            Password = "hashedOldPassword",
//            RequiresPasswordChange = true
//        };

//        _passwordHashMock
//            .Setup(x => x.VerifyPassword("wrongPassword", user.Password))
//            .Returns(false);

//        // Act
//        var result = await _updateUserFirstTime.Update(user, "wrongPassword", "newPass123!", "192.168.1.1");

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal("La contraseña actual es incorrecta", result.Message);
//    }

//    [Theory]
//    [InlineData("short")]
//    [InlineData("nouppercase123!")]
//    [InlineData("NOLOWERCASE123!")]
//    [InlineData("NoDigits!!!")]
//    [InlineData("NoSpecial123")]
//    public async Task Update_WithWeakPassword_ShouldReturnFailure(string weakPassword)
//    {
//        // Arrange
//        var user = new User
//        {
//            Id = 1,
//            Password = "hashedOldPassword",
//            RequiresPasswordChange = true
//        };

//        _passwordHashMock
//            .Setup(x => x.VerifyPassword("oldPassword", user.Password))
//            .Returns(true);

//        // Act
//        var result = await _updateUserFirstTime.Update(user, "oldPassword", weakPassword, "192.168.1.1");

//        // Assert
//        Assert.False(result.Success);
//        Assert.Contains("al menos 8 caracteres", result.Message);
//    }

//    [Fact]
//    public async Task Update_WithValidData_ShouldReturnSuccessAndNewTokens()
//    {
//        // Arrange
//        var role = new Role { Id = 4, Description = "Inquilino" };
//        var user = new User
//        {
//            Id = 1,
//            Mail = "user@example.com",
//            Password = "hashedOldPassword",
//            Role_id = 4,
//            RequiresPasswordChange = true,
//            Name = "John",
//            LastName = "Doe",
//            Dni = 12345678,
//            Photo = "/uploads/photo.jpg"
//        };

//        var newAccessToken = "new.jwt.token";
//        var newRefreshToken = "new.refresh.token";
//        var newHashedPassword = "newHashedPassword";

//        _passwordHashMock
//            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
//            .Returns(true);

//        _passwordHashMock
//            .Setup(x => x.HashPassword("NewPass123!"))
//            .Returns(newHashedPassword);

//        _roleRepositoryMock
//            .Setup(x => x.GetById(4))
//            .ReturnsAsync(role);

//        _jwtTokenGeneratorMock
//            .Setup(x => x.Generate(user.Id, user.Mail, user.Role_id, role.Description, false))
//            .Returns(newAccessToken);

//        _refreshTokenGeneratorMock
//            .Setup(x => x.Generate())
//            .Returns(newRefreshToken);

//        _userRepositoryMock
//            .Setup(x => x.Update(It.IsAny<User>()))
//            .Returns(Task.CompletedTask);

//        _refreshTokenRepositoryMock
//            .Setup(x => x.Add(It.IsAny<RefreshToken>()))
//            .ReturnsAsync((RefreshToken rt) => rt);

//        // Act
//        var result = await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

//        // Assert
//        Assert.True(result.Success);
//        Assert.Contains("Bienvenido a Foraria", result.Message);
//        Assert.Equal(newAccessToken, result.AccessToken);
//        Assert.Equal(newRefreshToken, result.RefreshToken);

//        _userRepositoryMock.Verify(x => x.Update(It.Is<User>(u =>
//            u.Password == newHashedPassword &&
//            u.RequiresPasswordChange == false
//        )), Times.Once);

//        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<RefreshToken>(rt =>
//            rt.UserId == 1 &&
//            rt.Token == newRefreshToken &&
//            rt.CreatedByIp == "192.168.1.1"
//        )), Times.Once);
//    }

//    [Fact]
//    public async Task Update_WhenRoleNotFound_ShouldReturnFailure()
//    {
//        // Arrange
//        var user = new User
//        {
//            Id = 1,
//            Mail = "user@example.com",
//            Password = "hashedOldPassword",
//            Role_id = 999,
//            RequiresPasswordChange = true
//        };

//        _passwordHashMock
//            .Setup(x => x.VerifyPassword("OldPass123!", user.Password))
//            .Returns(true);

//        _passwordHashMock
//            .Setup(x => x.HashPassword("NewPass123!"))
//            .Returns("newHashedPassword");

//        _userRepositoryMock
//            .Setup(x => x.Update(It.IsAny<User>()))
//            .Returns(Task.CompletedTask);

//        _roleRepositoryMock
//            .Setup(x => x.GetById(999))
//            .ReturnsAsync((Role?)null);

//        // Act
//        var result = await _updateUserFirstTime.Update(user, "OldPass123!", "NewPass123!", "192.168.1.1");

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal("User role not found", result.Message);
//    }
//}