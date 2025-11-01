using Xunit;
using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Foraria.Domain.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
    public async Task Register_DuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "existing@test.com",
            PhoneNumber = 1234567890,
            Role_id = 1
        };

        _mockUserRepo.Setup(r => r.ExistsEmail("existing@test.com")).ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email is already registered in the system", result.Message);
        Assert.Null(result.Id);
        Assert.Null(result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.ExistsEmail("existing@test.com"), Times.Once);
        _mockRoleRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        _mockResidenceRepo.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _mockEmailService.Verify(s => s.SendWelcomeEmail(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidRole_ShouldReturnError()
    {
        // Arrange
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "juan@test.com",
            PhoneNumber = 1234567890,
            Role_id = 999
        };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(999)).ReturnsAsync((Role?)null);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Selected role is not valid", result.Message);
        Assert.Null(result.Id);
        Assert.Null(result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.ExistsEmail("juan@test.com"), Times.Once);
        _mockRoleRepo.Verify(r => r.GetById(999), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidResidenceId_ShouldReturnError()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "juan@test.com",
            PhoneNumber = 1234567890,
            Role_id = 1
        };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(999)).ReturnsAsync(false);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Residence with ID 999 does not exist", result.Message);
        Assert.Null(result.Id);
        Assert.Null(result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.ExistsEmail("juan@test.com"), Times.Once);
        _mockRoleRepo.Verify(r => r.GetById(1), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(999), Times.Once);
        _mockResidenceRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_ResidenceGetByIdReturnsNull_ShouldReturnError()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "juan@test.com",
            PhoneNumber = 1234567890,
            Role_id = 1
        };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync((Residence?)null);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error retrieving residence information", result.Message);
        Assert.Null(result.Id);
        Assert.Null(result.TemporaryPassword);

        _mockResidenceRepo.Verify(r => r.Exists(5), Times.Once);
        _mockResidenceRepo.Verify(r => r.GetById(5), Times.Once);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_RoleAlreadyAssignedToResidence_ShouldReturnError()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Propietario" };
        var user = new User
        {
            Name = "María",
            LastName = "González",
            Mail = "maria@test.com",
            PhoneNumber = 9876543210,
            Role_id = 2,
            Role = role
        };
        var residence = new Residence { Id = 5, Number = 205, Floor = 2, Tower = "B" };

        _mockUserRepo.Setup(r => r.ExistsEmail("maria@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Propietario")).ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("This residence already has a user with the role 'Propietario' assigned. Each residence can only have one user per role.", result.Message);
        Assert.Null(result.Id);
        Assert.Null(result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.ExistsUserWithRoleInResidence(5, "Propietario"), Times.Once);
        _mockPasswordGenerator.Verify(s => s.Generate(), Times.Never);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_EmailSendFails_ShouldStillReturnSuccess()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "juan@test.com",
            PhoneNumber = 1234567890,
            Role_id = 1,
            Role = role
        };
        var residence = new Residence { Id = 5, Number = 205, Floor = 2, Tower = "B" };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(s => s.Generate()).ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword("TempPass123!")).Returns("$2a$11$hashedpassword");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockEmailService.Setup(s => s.SendWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP server not available"));

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User registered successfully. An email has been sent with the credentials.", result.Message);
        Assert.Equal(1, result.Id);
        Assert.Equal("TempPass123!", result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _mockEmailService.Verify(s => s.SendWelcomeEmail("juan@test.com", "Juan", "Pérez", "TempPass123!"), Times.Once);
    }

    [Fact]
    public async Task Register_SetsRequiresPasswordChangeToTrue()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User
        {
            Name = "Juan",
            LastName = "Pérez",
            Mail = "juan@test.com",
            PhoneNumber = 1234567890,
            Role_id = 1,
            Role = role,
            RequiresPasswordChange = false // Verificamos que se cambia a true
        };
        var residence = new Residence { Id = 5, Number = 205, Floor = 2, Tower = "B" };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.Exists(5)).ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(s => s.Generate()).ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword("TempPass123!")).Returns("$2a$11$hashedpassword");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 1; return u; });

        var service = CreateService();

        // Act
        await service.Register(user, 5);

        // Assert
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u =>
            u.RequiresPasswordChange == true
        )), Times.Once);
    }


    [Fact]
    public async Task GetAllUsersInNumber_ShouldReturnTotalUsers()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetAllInNumber()).ReturnsAsync(42);

        var service = CreateService();

        // Act
        var result = await service.GetAllUsersInNumber();

        // Assert
        Assert.Equal(42, result);
        _mockUserRepo.Verify(r => r.GetAllInNumber(), Times.Once);
    }
}