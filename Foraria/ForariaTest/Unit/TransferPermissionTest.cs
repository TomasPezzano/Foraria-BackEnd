//using Foraria.Application.UseCase;
//using Foraria.Domain.Repository;
//using ForariaDomain;
//using Moq;
//using Xunit;

//namespace Foraria.Test.Application.UseCase;

//public class TransferPermissionTests
//{
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
//    private readonly TransferPermission _useCase;

//    public TransferPermissionTests()
//    {
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
//        _useCase = new TransferPermission(
//            _userRepositoryMock.Object,
//            _refreshTokenRepositoryMock.Object);
//    }

//    [Fact]
//    public async Task Execute_WhenOwnerNotFound_ReturnsFailure()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync((User?)null);

        
//        var result = await _useCase.Execute(ownerId, tenantId);

      
//        Assert.False(result.Success);
//        Assert.Equal("Usuario no encontrado", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllByUserId(It.IsAny<int>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenTenantNotFound_ReturnsFailure()
//    {
        
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync((User?)null);

      
//        var result = await _useCase.Execute(ownerId, tenantId);

       
//        Assert.False(result.Success);
//        Assert.Equal("Usuario no encontrado", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenOwnerIsNotPropietario_ReturnsFailure()
//    {
    
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = new User
//        {
//            Id = ownerId,
//            Name = "Juan",
//            LastName = "Perez",
//            Mail = "juan@test.com",
//            HasPermission = true,
//            Role = new Role { Id = 3, Description = "Administrador" },
//            Role_id = 3,
//            Residences = new List<Residence>
//            {
//                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
//            }
//        };

//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

    
//        var result = await _useCase.Execute(ownerId, tenantId);

      
//        Assert.False(result.Success);
//        Assert.Equal("Solo los propietarios pueden transferir permisos", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenTenantIsNotInquilino_ReturnsFailure()
//    {
       
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);

//        var tenant = new User
//        {
//            Id = tenantId,
//            Name = "Maria",
//            LastName = "Garcia",
//            Mail = "maria@test.com",
//            HasPermission = false,
//            Role = new Role { Id = 1, Description = "Propietario" },
//            Role_id = 1,
//            Residences = new List<Residence>
//            {
//                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
//            }
//        };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

      
//        var result = await _useCase.Execute(ownerId, tenantId);

     
//        Assert.False(result.Success);
//        Assert.Equal("Los permisos solo pueden transferirse a inquilinos", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenUsersDoNotShareResidence_ReturnsFailure()
//    {
       
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = new User
//        {
//            Id = ownerId,
//            Name = "Juan",
//            LastName = "Perez",
//            Mail = "juan@test.com",
//            HasPermission = true,
//            Role = new Role { Id = 1, Description = "Propietario" },
//            Role_id = 1,
//            Residences = new List<Residence>
//            {
//                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
//            }
//        };

//        var tenant = new User
//        {
//            Id = tenantId,
//            Name = "Maria",
//            LastName = "Garcia",
//            Mail = "maria@test.com",
//            HasPermission = false,
//            Role = new Role { Id = 2, Description = "Inquilino" },
//            Role_id = 2,
//            Residences = new List<Residence>
//            {
//                new Residence { Id = 2, Number = 202, Floor = 2, Tower = "B" } 
//            }
//        };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

       
//        var result = await _useCase.Execute(ownerId, tenantId);

        
//        Assert.False(result.Success);
//        Assert.Equal("El propietario y el inquilino deben compartir al menos una residencia", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenOwnerDoesNotHavePermission_ReturnsFailure()
//    {
      
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false); 
//        var tenant = CreateTenant(tenantId, hasPermission: false);

        
//        var sharedResidence = new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" };
//        owner.Residences = new List<Residence> { sharedResidence };
//        tenant.Residences = new List<Residence> { sharedResidence };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        var result = await _useCase.Execute(ownerId, tenantId);

     
//        Assert.False(result.Success);
//        Assert.Equal("El propietario no tiene permisos de votación para transferir", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenAllValidationsPass_TransfersPermissionsSuccessfully()
//    {
      
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);
//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        var sharedResidence = new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" };
//        owner.Residences = new List<Residence> { sharedResidence };
//        tenant.Residences = new List<Residence> { sharedResidence };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        _userRepositoryMock
//            .Setup(x => x.Update(It.IsAny<User>()))
//            .Returns(Task.CompletedTask);

//        _refreshTokenRepositoryMock
//            .Setup(x => x.RevokeAllByUserId(tenantId))
//            .Returns(Task.CompletedTask);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.True(result.Success);
//        Assert.Equal("Permisos transferidos exitosamente. El inquilino debe iniciar sesión nuevamente para obtener los nuevos permisos.", result.Message);
//        Assert.Equal(ownerId, result.OwnerId);
//        Assert.Equal(tenantId, result.TenantId);
//        Assert.False(result.OwnerHasPermission);
//        Assert.True(result.TenantHasPermission);

//        Assert.False(owner.HasPermission);
//        Assert.True(tenant.HasPermission);

//        _userRepositoryMock.Verify(x => x.Update(owner), Times.Once);
//        _userRepositoryMock.Verify(x => x.Update(tenant), Times.Once);
//        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllByUserId(tenantId), Times.Once);
//    }

//    [Fact]
//    public async Task Execute_WhenUsersShareMultipleResidences_TransfersPermissionsSuccessfully()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);
//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        var residence1 = new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" };
//        var residence2 = new Residence { Id = 2, Number = 102, Floor = 1, Tower = "A" };
//        var residence3 = new Residence { Id = 3, Number = 201, Floor = 2, Tower = "B" };

//        owner.Residences = new List<Residence> { residence1, residence2 };
//        tenant.Residences = new List<Residence> { residence2, residence3 }; 

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        _userRepositoryMock
//            .Setup(x => x.Update(It.IsAny<User>()))
//            .Returns(Task.CompletedTask);

//        _refreshTokenRepositoryMock
//            .Setup(x => x.RevokeAllByUserId(tenantId))
//            .Returns(Task.CompletedTask);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.True(result.Success);
//        Assert.False(owner.HasPermission);
//        Assert.True(tenant.HasPermission);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Exactly(2));
//    }

//    [Fact]
//    public async Task Execute_WhenOwnerHasNoResidences_ReturnsFailure()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);
//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        owner.Residences = new List<Residence>(); 
//        tenant.Residences = new List<Residence>
//        {
//            new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
//        };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.False(result.Success);
//        Assert.Equal("El propietario y el inquilino deben compartir al menos una residencia", result.Message);
//    }

//    [Fact]
//    public async Task Execute_WhenTenantHasNoResidences_ReturnsFailure()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true);
//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        owner.Residences = new List<Residence>
//        {
//            new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
//        };
//        tenant.Residences = new List<Residence>(); 

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.False(result.Success);
//        Assert.Equal("El propietario y el inquilino deben compartir al menos una residencia", result.Message);
//    }

    
//    private User CreateOwner(int id, bool hasPermission)
//    {
//        return new User
//        {
//            Id = id,
//            Name = "Juan",
//            LastName = "Perez",
//            Mail = $"owner{id}@test.com",
//            HasPermission = hasPermission,
//            Role = new Role { Id = 1, Description = "Propietario" },
//            Role_id = 1,
//            Residences = new List<Residence>()
//        };
//    }

//    private User CreateTenant(int id, bool hasPermission)
//    {
//        return new User
//        {
//            Id = id,
//            Name = "Maria",
//            LastName = "Garcia",
//            Mail = $"tenant{id}@test.com",
//            HasPermission = hasPermission,
//            Role = new Role { Id = 2, Description = "Inquilino" },
//            Role_id = 2,
//            Residences = new List<Residence>()
//        };
//    }
//}