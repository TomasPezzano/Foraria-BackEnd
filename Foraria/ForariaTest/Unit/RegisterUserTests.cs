using Xunit;
using Moq;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

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
    public async Task Register_ValidUser_ShouldReturnSuccess()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User registered successfully. An email has been sent with the credentials.", result.Message);
        Assert.Equal(1, result.Id);
        Assert.Equal("TempPass123!", result.TemporaryPassword);

        _mockUserRepo.Verify(r => r.ExistsEmail("juan@test.com"), Times.Once);
        _mockRoleRepo.Verify(r => r.Exists(1), Times.Once);
        _mockPasswordHash.Verify(s => s.HashPassword("TempPass123!"), Times.Once);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _mockEmailService.Verify(s => s.SendWelcomeEmail(
            "juan@test.com",
            "Juan",
            "Pérez",
            "TempPass123!"
        ), Times.Once);
    }

    [Fact]
    public async Task Register_ValidUserWithResidences_ShouldReturnSuccess()
    {
        // Arrange
        var residenceDtos = new List<ResidenceDto>
        {
            new ResidenceDto { Id = 1, Number = 101, Floor = 1, Tower = "A" },
            new ResidenceDto { Id = 2, Number = 102, Floor = 1, Tower = "A" }
        };

        var residence1 = new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" };
        var residence2 = new Residence { Id = 2, Number = 102, Floor = 1, Tower = "A" };

        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.Exists(1))
                          .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.Exists(2))
                          .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(1))
                          .ReturnsAsync(residence1);
        _mockResidenceRepo.Setup(r => r.GetById(2))
                          .ReturnsAsync(residence2);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1,
            Residences = residenceDtos
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User registered successfully. An email has been sent with the credentials.", result.Message);
        Assert.Equal(1, result.Id);
        Assert.Equal("TempPass123!", result.TemporaryPassword);
        Assert.NotNull(result.Residences);
        Assert.Equal(2, result.Residences.Count);
        Assert.Equal(1, result.Residences[0].Id);
        Assert.Equal(101, result.Residences[0].Number);
        Assert.Equal(2, result.Residences[1].Id);
        Assert.Equal(102, result.Residences[1].Number);

        _mockUserRepo.Verify(r => r.ExistsEmail("juan@test.com"), Times.Once);
        _mockRoleRepo.Verify(r => r.Exists(1), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(1), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(2), Times.Once);
        _mockResidenceRepo.Verify(r => r.GetById(1), Times.Once);
        _mockResidenceRepo.Verify(r => r.GetById(2), Times.Once);
        _mockPasswordHash.Verify(s => s.HashPassword("TempPass123!"), Times.Once);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.Residence.Count == 2)), Times.Once);
        _mockEmailService.Verify(s => s.SendWelcomeEmail(
            "juan@test.com",
            "Juan",
            "Pérez",
            "TempPass123!"
        ), Times.Once);
    }

    [Fact]
    public async Task Register_ValidUserWithSingleResidence_ShouldReturnSuccess()
    {
        // Arrange
        var residenceDtos = new List<ResidenceDto>
        {
            new ResidenceDto { Id = 5, Number = 205, Floor = 2, Tower = "B" }
        };

        var residence = new Residence { Id = 5, Number = 205, Floor = 2, Tower = "B" };

        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.Exists(5))
                          .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.GetById(5))
                          .ReturnsAsync(residence);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "María",
            LastName = "González",
            Email = "maria@test.com",
            Phone = "9876543210",
            RoleId = 2,
            Residences = residenceDtos
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Residences);
        Assert.Single(result.Residences);
        Assert.Equal(5, result.Residences[0].Id);
        Assert.Equal(205, result.Residences[0].Number);
        Assert.Equal(2, result.Residences[0].Floor);
        Assert.Equal("B", result.Residences[0].Tower);

        _mockResidenceRepo.Verify(r => r.Exists(5), Times.Once);
        _mockResidenceRepo.Verify(r => r.GetById(5), Times.Once);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.Residence.Count == 1)), Times.Once);
    }


    [Fact]
    public async Task Register_InvalidResidenceId_ShouldReturnError()
    {
        // Arrange
        var residenceDtos = new List<ResidenceDto>
    {
        new ResidenceDto { Id = 999, Number = 102, Floor = 1, Tower = "A" }
    };

        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockResidenceRepo.Setup(r => r.Exists(999))
                          .ReturnsAsync(false);

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1,
            Residences = residenceDtos
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Residence with ID 999 does not exist", result.Message);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _mockResidenceRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
        _mockEmailService.Verify(s => s.SendWelcomeEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task Register_ResidenceWithoutId_ShouldReturnError()
    {
        // Arrange
        var residenceDtos = new List<ResidenceDto>
        {
            new ResidenceDto { Id = null, Number = 101, Floor = 1, Tower = "A" }
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1,
            Residences = residenceDtos
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("All residences must have a valid ID", result.Message);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _mockResidenceRepo.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Register_EmptyResidenceList_ShouldReturnSuccess()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1,
            Residences = new List<ResidenceDto>()
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.Residence.Count == 0)), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
        _mockResidenceRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Register_NullResidenceList_ShouldReturnSuccess()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 1,
            Residences = null
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _mockResidenceRepo.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
        _mockResidenceRepo.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturnError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(true);

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "existing@test.com",
            Phone = "1234567890",
            RoleId = 1
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email is already registered in the system", result.Message);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _mockEmailService.Verify(s => s.SendWelcomeEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidRole_ShouldReturnError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(false);

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "1234567890",
            RoleId = 999
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Selected role is not valid", result.Message);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidPhone_ShouldReturnError()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "abc-def-ghij",
            RoleId = 1
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Phone format is not valid", result.Message);

        _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_PhoneWithSpaces_ShouldReturnSuccess()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "123 456 7890",
            RoleId = 1
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.PhoneNumber == 1234567890)), Times.Once);
    }

    [Fact]
    public async Task Register_PhoneWithDashes_ShouldReturnSuccess()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });
        _mockRoleRepo.Setup(r => r.Exists(It.IsAny<int>()))
                     .ReturnsAsync(true);
        _mockPasswordGenerator.Setup(s => s.Generate())
                              .ReturnsAsync("TempPass123!");
        _mockPasswordHash.Setup(s => s.HashPassword(It.IsAny<string>()))
                         .Returns("$2a$11$hashedpassword");

        var service = CreateService();

        var userDto = new UserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan@test.com",
            Phone = "123-456-7890",
            RoleId = 1
        };

        // Act
        var result = await service.Register(userDto);

        // Assert
        Assert.True(result.Success);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.PhoneNumber == 1234567890)), Times.Once);
    }

}