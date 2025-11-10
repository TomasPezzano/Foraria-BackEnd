using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Foraria.Domain.Service;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit;

public class RegisterUserTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IRoleRepository> _mockRoleRepo;
    private readonly Mock<IPasswordHash> _mockPasswordHash;
    private readonly Mock<IGeneratePassword> _mockPasswordGenerator;
    private readonly Mock<ISendEmail> _mockEmailService;
    private readonly Mock<IResidenceRepository> _mockResidenceRepo;

    public RegisterUserTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockRoleRepo = new Mock<IRoleRepository>();
        _mockPasswordHash = new Mock<IPasswordHash>();
        _mockPasswordGenerator = new Mock<IGeneratePassword>();
        _mockEmailService = new Mock<ISendEmail>();
        _mockResidenceRepo = new Mock<IResidenceRepository>();
    }

    private RegisterUser CreateService()
    {
        return new RegisterUser(
            _mockUserRepo.Object,
            _mockPasswordGenerator.Object,
            _mockRoleRepo.Object,
            _mockPasswordHash.Object,
            _mockEmailService.Object,
            _mockResidenceRepo.Object
        );
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldThrowBusinessException()
    {
        // Arrange
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "existing@test.com",
            Role_id = 1
        };

        _mockUserRepo.Setup(r => r.ExistsEmail("existing@test.com")).ReturnsAsync(true);
        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, 5));
        Assert.Equal("Email is already registered in the system", ex.Message);
    }

    [Fact]
    public async Task Register_InvalidRole_ShouldThrowNotFoundException()
    {
        var user = new User { Mail = "juan@test.com", Role_id = 99 };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(99)).ReturnsAsync((Role?)null);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.Register(user, 1));
        Assert.Equal("Selected role is not valid", ex.Message);
    }

    [Fact]
    public async Task Register_InvalidResidence_ShouldThrowNotFoundException()
    {
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User { Mail = "test@mail.com", Role_id = 1 };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(false);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.Register(user, 5));
        Assert.Equal("Residence with ID 5 does not exist", ex.Message);
    }

    [Fact]
    public async Task Register_ResidenceGetByIdReturnsNull_ShouldThrowBusinessException()
    {
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User { Mail = "juan@test.com", Role_id = 1 };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync((Residence?)null);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, 5));
        Assert.Equal("Error retrieving residence information", ex.Message);
    }

    [Fact]
    public async Task Register_RoleAlreadyAssigned_ShouldThrowBusinessException()
    {
        var role = new Role { Id = 1, Description = "Propietario" };
        var user = new User { Mail = "juan@test.com", Role_id = 1 };
        var residence = new Residence { Id = 5, Number = 205 };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Propietario")).ReturnsAsync(true);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, 5));
        Assert.Equal("This residence already has a user with the role 'Propietario' assigned.", ex.Message);
    }

    [Fact]
    public async Task Register_EmailSendFails_ShouldStillReturnUser()
    {
        var role = new Role { Id = 1, Description = "Inquilino" };
        var residence = new Residence { Id = 5, Number = 205 };
        var user = new User { Mail = "juan@test.com", Role_id = 1, Name = "Juan", LastName = "Perez" };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.HashPassword("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 10; return u; });
        _mockEmailService.Setup(s => s.SendWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("SMTP error"));

        var service = CreateService();

        var result = await service.Register(user, 5);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("juan@test.com", result.Mail);
        Assert.Equal("Temp123", result.Password);
    }

    [Fact]
    public async Task Register_SetsRequiresPasswordChangeTrue()
    {
        var role = new Role { Id = 1, Description = "Inquilino" };
        var residence = new Residence { Id = 5, Number = 205 };
        var user = new User { Mail = "test@mail.com", Role_id = 1 };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.HashPassword("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var service = CreateService();

        var result = await service.Register(user, 5);

        Assert.True(result.RequiresPasswordChange);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.RequiresPasswordChange)), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersInNumber_ShouldReturnTotalUsers()
    {
        _mockUserRepo.Setup(r => r.GetAllInNumber()).ReturnsAsync(42);
        var service = CreateService();

        var result = await service.GetAllUsersInNumber();

        Assert.Equal(42, result);
    }
}
