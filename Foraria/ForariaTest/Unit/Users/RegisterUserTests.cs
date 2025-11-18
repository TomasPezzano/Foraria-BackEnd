// Ubicación: ForariaTest/Unit/Users/RegisterUserTests.cs
using Moq;
using Foraria.Domain.Repository;
using ForariaDomain;
using Foraria.Domain.Service;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.Users;

public class RegisterUserTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IRoleRepository> _mockRoleRepo;
    private readonly Mock<IPasswordHash> _mockPasswordHash;
    private readonly Mock<IGeneratePassword> _mockPasswordGenerator;
    private readonly Mock<ISendEmail> _mockEmailService;
    private readonly Mock<IResidenceRepository> _mockResidenceRepo;
    private readonly Mock<IConsortiumRepository> _mockConsortiumRepo;

    public RegisterUserTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockRoleRepo = new Mock<IRoleRepository>();
        _mockPasswordHash = new Mock<IPasswordHash>();
        _mockPasswordGenerator = new Mock<IGeneratePassword>();
        _mockEmailService = new Mock<ISendEmail>();
        _mockResidenceRepo = new Mock<IResidenceRepository>();
        _mockConsortiumRepo = new Mock<IConsortiumRepository>();
    }

    private RegisterUser CreateService()
    {
        return new RegisterUser(
            _mockUserRepo.Object,
            _mockPasswordGenerator.Object,
            _mockRoleRepo.Object,
            _mockPasswordHash.Object,
            _mockEmailService.Object,
            _mockResidenceRepo.Object,
            _mockConsortiumRepo.Object
        );
    }

    #region Tests Existentes Corregidos

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
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, 5, null));
        Assert.Equal("El email ya está registrado en el sistema.", ex.Message);
    }

    [Fact]
    public async Task Register_InvalidRole_ShouldThrowNotFoundException()
    {
        // Arrange
        var user = new User { Mail = "juan@test.com", Role_id = 99 };

        _mockUserRepo.Setup(r => r.ExistsEmail("juan@test.com")).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(99)).ReturnsAsync((Role?)null);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.Register(user, 1, null));
        Assert.Equal("El rol seleccionado no es válido.", ex.Message);
    }

    [Fact]
    public async Task Register_InvalidResidence_ShouldThrowNotFoundException()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var user = new User { Mail = "test@mail.com", Role_id = 1 };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa GetByIdWithoutFilters
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync((Residence?)null);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.Register(user, 5, null));
        Assert.Equal("Residencia con ID 5 no existe.", ex.Message);
    }

    [Fact]
    public async Task Register_RoleAlreadyAssigned_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Propietario" };
        var user = new User { Mail = "juan@test.com", Role_id = 1 };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            ConsortiumId = 1,
            Floor = 2,
            Tower = "A"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa GetByIdWithoutFilters
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);

        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Propietario")).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, 5, null));
        Assert.Equal("Esta residencia ya tiene un usuario con el rol 'Propietario' asignado.", ex.Message);
    }

    [Fact]
    public async Task Register_EmailSendFails_ShouldStillReturnUser()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "juan@test.com",
            Role_id = 1,
            Name = "Juan",
            LastName = "Perez"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa GetByIdWithoutFilters
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);

        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 10; return u; });
        _mockEmailService.Setup(s => s.SendWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("SMTP error"));

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("juan@test.com", result.Mail);
        Assert.Equal("Temp123", result.Password);
    }

    [Fact]
    public async Task Register_SetsRequiresPasswordChangeTrue()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Inquilino" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "test@mail.com",
            Role_id = 1,
            Name = "Test",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa GetByIdWithoutFilters
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);

        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.True(result.RequiresPasswordChange);
        _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.RequiresPasswordChange)), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersInNumber_ShouldReturnTotalUsers()
    {
        // Arrange
        int consortiumId = 123;
        _mockUserRepo.Setup(r => r.GetAllInNumber()).ReturnsAsync(42);

        var service = CreateService();

        // Act
        var result = await service.GetAllUsersInNumber();

        // Assert
        Assert.Equal(42, result);
    }

    #endregion

    #region Tests para Administrador

    [Fact]
    public async Task Register_Administrator_WithoutConsortiumId_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Admin",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, null, null));
        Assert.Equal("El rol Administrador requiere un Consortium ID.", ex.Message);
    }

    [Fact]
    public async Task Register_Administrator_WithInvalidConsortiumId_ShouldThrowNotFoundException()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Admin",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa ExistsWithoutFilters
        _mockConsortiumRepo.Setup(r => r.ExistsWithoutFilters(999)).ReturnsAsync(false);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.Register(user, null, 999));
        Assert.Equal("Consorcio con ID 999 no existe.", ex.Message);
    }

    [Fact]
    public async Task Register_Administrator_ConsortiumAlreadyHasAdmin_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Admin",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa ExistsWithoutFilters
        _mockConsortiumRepo.Setup(r => r.ExistsWithoutFilters(1)).ReturnsAsync(true);
        _mockConsortiumRepo.Setup(r => r.HasAdministrator(1)).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, null, 1));
        Assert.Equal("El consorcio ya tiene un administrador asignado.", ex.Message);
    }

    [Fact]
    public async Task Register_Administrator_Success_ShouldAssignToConsortium()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Admin",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);

        // ✅ CAMBIO: Ahora usa ExistsWithoutFilters
        _mockConsortiumRepo.Setup(r => r.ExistsWithoutFilters(1)).ReturnsAsync(true);
        _mockConsortiumRepo.Setup(r => r.HasAdministrator(1)).ReturnsAsync(false);

        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 20; return u; });
        _mockConsortiumRepo.Setup(r => r.AssignAdministrator(1, 20)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.Register(user, null, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.Id);
        Assert.Equal("admin@test.com", result.Mail);
        Assert.Empty(result.Residences); // Administrador no tiene residencias
        _mockConsortiumRepo.Verify(r => r.AssignAdministrator(1, 20), Times.Once);
    }

    [Fact]
    public async Task Register_Administrator_Success_ShouldNotHaveResidences()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Admin",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);
        _mockConsortiumRepo.Setup(r => r.ExistsWithoutFilters(1)).ReturnsAsync(true);
        _mockConsortiumRepo.Setup(r => r.HasAdministrator(1)).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 20; return u; });
        _mockConsortiumRepo.Setup(r => r.AssignAdministrator(1, 20)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.Register(user, null, 1);

        // Assert
        Assert.NotNull(result.Residences);
        Assert.Empty(result.Residences); // Administradores NO tienen residencias

        _mockUserRepo.Verify(
            r => r.Add(It.Is<User>(u => u.Residences != null && u.Residences.Count == 0)),
            Times.Once
        );
    }

    [Fact]
    public async Task Register_Administrator_SendsWelcomeEmail()
    {
        // Arrange
        var role = new Role { Id = 2, Description = "Administrador" };
        var user = new User
        {
            Mail = "admin@test.com",
            Role_id = 2,
            Name = "Carlos",
            LastName = "Admin"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(2)).ReturnsAsync(role);
        _mockConsortiumRepo.Setup(r => r.ExistsWithoutFilters(1)).ReturnsAsync(true);
        _mockConsortiumRepo.Setup(r => r.HasAdministrator(1)).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 20; return u; });
        _mockConsortiumRepo.Setup(r => r.AssignAdministrator(1, 20)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.Register(user, null, 1);

        // Assert
        _mockEmailService.Verify(
            s => s.SendWelcomeEmail("admin@test.com", "Carlos", "Admin", "Temp123"),
            Times.Once
        );
    }

    #endregion

    #region Tests para Propietario/Inquilino

    [Fact]
    public async Task Register_Propietario_WithoutResidenceId_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 3, Description = "Propietario" };
        var user = new User
        {
            Mail = "owner@test.com",
            Role_id = 3,
            Name = "Owner",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(3)).ReturnsAsync(role);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, null, null));
        Assert.Equal("El rol 'Propietario' requiere una residencia asignada.", ex.Message);
    }

    [Fact]
    public async Task Register_Inquilino_WithoutResidenceId_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 4, Description = "Inquilino" };
        var user = new User
        {
            Mail = "tenant@test.com",
            Role_id = 4,
            Name = "Tenant",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(4)).ReturnsAsync(role);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, null, null));
        Assert.Equal("El rol 'Inquilino' requiere una residencia asignada.", ex.Message);
    }

    [Fact]
    public async Task Register_Propietario_Success_ShouldHaveHasPermissionTrue()
    {
        // Arrange
        var role = new Role { Id = 3, Description = "Propietario" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "owner@test.com",
            Role_id = 3,
            Name = "Owner",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(3)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Propietario")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 30; return u; });

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.True(result.HasPermission); // Propietarios tienen HasPermission = true

        _mockUserRepo.Verify(
            r => r.Add(It.Is<User>(u => u.HasPermission == true)),
            Times.Once
        );
    }

    [Fact]
    public async Task Register_Propietario_Success_ReturnsResidenceInformation()
    {
        // Arrange
        var role = new Role { Id = 3, Description = "Propietario" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "owner@test.com",
            Role_id = 3,
            Name = "Owner",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(3)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Propietario")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 30; return u; });

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.NotEmpty(result.Residences);
        Assert.Single(result.Residences);

        var returnedResidence = result.Residences.First();
        Assert.Equal(5, returnedResidence.Id);
        Assert.Equal(205, returnedResidence.Number);
        Assert.Equal(2, returnedResidence.Floor);
        Assert.Equal("A", returnedResidence.Tower);
        Assert.Equal(1, returnedResidence.ConsortiumId);
    }

    #endregion

    #region Tests para Consejo

    [Fact]
    public async Task Register_Consejo_WithoutResidenceId_ShouldThrowBusinessException()
    {
        // Arrange
        var role = new Role { Id = 1, Description = "Consejo" };
        var user = new User
        {
            Mail = "consejo@test.com",
            Role_id = 1,
            Name = "Consejo",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(1)).ReturnsAsync(role);

        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.Register(user, null, null));
        Assert.Equal("El rol 'Consejo' requiere una residencia asignada.", ex.Message);
    }

    #endregion

    #region Tests de Password y Seguridad

    [Fact]
    public async Task Register_Success_GeneratesTemporaryPassword()
    {
        // Arrange
        var role = new Role { Id = 4, Description = "Inquilino" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "test@mail.com",
            Role_id = 4,
            Name = "Test",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(4)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);

        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("TempPass999");
        _mockPasswordHash.Setup(h => h.Execute("TempPass999")).Returns("hashed_TempPass999");

        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 10; return u; });

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.Equal("TempPass999", result.Password); // Retorna el password sin hashear

        _mockPasswordGenerator.Verify(g => g.Generate(), Times.Once);
        _mockPasswordHash.Verify(h => h.Execute("TempPass999"), Times.Once);

        _mockUserRepo.Verify(
            r => r.Add(It.Is<User>(u => u.Password == "hashed_TempPass999")), // Se guarda hasheado
            Times.Once
        );
    }

    [Fact]
    public async Task Register_Success_AllUsersRequirePasswordChange()
    {
        // Arrange
        var role = new Role { Id = 4, Description = "Inquilino" };
        var residence = new Residence
        {
            Id = 5,
            Number = 205,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };
        var user = new User
        {
            Mail = "test@mail.com",
            Role_id = 4,
            Name = "Test",
            LastName = "User"
        };

        _mockUserRepo.Setup(r => r.ExistsEmail(user.Mail)).ReturnsAsync(false);
        _mockRoleRepo.Setup(r => r.GetById(4)).ReturnsAsync(role);
        _mockResidenceRepo.Setup(r => r.GetByIdWithoutFilters(5)).ReturnsAsync(residence);
        _mockUserRepo.Setup(r => r.ExistsUserWithRoleInResidence(5, "Inquilino")).ReturnsAsync(false);
        _mockPasswordGenerator.Setup(g => g.Generate()).ReturnsAsync("Temp123");
        _mockPasswordHash.Setup(h => h.Execute("Temp123")).Returns("hashed");
        _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 10; return u; });

        var service = CreateService();

        // Act
        var result = await service.Register(user, 5, null);

        // Assert
        Assert.True(result.RequiresPasswordChange);
    }

    #endregion
}