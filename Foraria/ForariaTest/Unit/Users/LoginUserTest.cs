using Moq;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Aplication.Configuration;
using ForariaDomain.Exceptions;
using Microsoft.Extensions.Options;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit.Users;

public class LoginUserTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHash> _mockPasswordHash;
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
    private readonly Mock<IRefreshTokenGenerator> _mockRefreshTokenGenerator;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
    private readonly Mock<IRoleRepository> _mockRoleRepo;
    private readonly Mock<IGetConsortiumByAdminUser> _mockGetConsortiumByAdminUser;

    public LoginUserTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPasswordHash = new Mock<IPasswordHash>();
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
        _mockRefreshTokenGenerator = new Mock<IRefreshTokenGenerator>();
        _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockRoleRepo = new Mock<IRoleRepository>();
        _mockGetConsortiumByAdminUser = new Mock<IGetConsortiumByAdminUser>();

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
            _mockJwtSettings.Object,
            _mockRoleRepo.Object,
            _mockGetConsortiumByAdminUser.Object
        );
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "hashed",
            Role_id = 1,
            RequiresPasswordChange = true,
            HasPermission = false,
            Residences = new List<Residence>
        {
            new Residence { Id = 10, Number = 101, Floor = 1, Tower = "A", ConsortiumId = 5 }
        }
        };

        _mockPasswordHash.Setup(p => p.VerifyPassword("TempPass123!", user.Password)).Returns(true);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockJwtGenerator.Setup(j => j.Generate(1, "juan@test.com", 1, "Inquilino", true, false))
                         .Returns("mock.access.token");
        _mockRefreshTokenGenerator.Setup(r => r.Generate()).Returns("mock.refresh.token");
        _mockRefreshTokenRepo.Setup(r => r.Add(It.IsAny<RefreshToken>()))
                             .ReturnsAsync((RefreshToken rt) => rt);

        var service = CreateService();

        // Act
        var result = await service.Login(user, "TempPass123!", "192.168.1.1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock.access.token", result.AccessToken);
        Assert.Equal("mock.refresh.token", result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(1, result.User.Id);

        _mockPasswordHash.Verify(p => p.VerifyPassword("TempPass123!", user.Password), Times.Once);
        _mockRoleRepo.Verify(r => r.GetById(1), Times.Once);
        _mockJwtGenerator.Verify(j => j.Generate(1, "juan@test.com", 1, "Inquilino", true, false), Times.Once);
        _mockRefreshTokenRepo.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
    }


    [Fact]
    public async Task Login_ShouldThrow_WhenUserIsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.Login(null, "password", "ip");

        // Assert
        await Assert.ThrowsAsync<BusinessException>(act);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "hashed",
            Role_id = 1
        };

        _mockPasswordHash.Setup(p => p.VerifyPassword("wrong", user.Password)).Returns(false);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.Login(user, "wrong", "ip");

        // Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(act);
        Assert.Equal("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenRoleNotFound()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "hashed",
            Role_id = 999
        };

        _mockPasswordHash.Setup(p => p.VerifyPassword("pass", user.Password)).Returns(true);
        _mockRoleRepo.Setup(r => r.GetById(999)).ReturnsAsync((Role?)null);

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.Login(user, "pass", "ip");

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User role not found", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldSetCorrectRefreshTokenFields()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Id = 1,
            Mail = "juan@test.com",
            Password = "hashed",
            Role_id = 1,
            Residences = new List<Residence> // ✅ simulamos un usuario real con residencia
        {
            new Residence { Id = 10, Number = 101, Floor = 1, Tower = "A", ConsortiumId = 5 }
        }
        };

        _mockPasswordHash.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockJwtGenerator.Setup(j => j.Generate(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
                                               It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                         .Returns("token");
        _mockRefreshTokenGenerator.Setup(r => r.Generate()).Returns("refresh");
        _mockRefreshTokenRepo.Setup(r => r.Add(It.IsAny<RefreshToken>()))
                             .ReturnsAsync((RefreshToken rt) => rt);

        var service = CreateService();

        // Act
        await service.Login(user, "pass", "127.0.0.1");

        // Assert
        _mockRefreshTokenRepo.Verify(r => r.Add(It.Is<RefreshToken>(
            rt => rt.UserId == 1 &&
                  rt.Token == "refresh" &&
                  rt.CreatedByIp == "127.0.0.1" &&
                  rt.IsRevoked == false &&
                  rt.ExpiresAt > DateTime.Now
        )), Times.Once);
    }
}
