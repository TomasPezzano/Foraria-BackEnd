//using Foraria.Application.UseCase;
//using Foraria.Domain.Repository;
//using ForariaDomain;
//using Moq;
//using Xunit;

//namespace Foraria.Test.Application.UseCase;

//public class RevokePermissionTests
//{
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
//    private readonly RevokePermission _useCase;

//    public RevokePermissionTests()
//    {
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
//        _useCase = new RevokePermission(
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

//        var owner = CreateOwner(ownerId, hasPermission: false);

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
//            HasPermission = false,
//            Role = new Role { Id = 3, Description = "Administrador" },
//            Role_id = 3
//        };

//        var tenant = CreateTenant(tenantId, hasPermission: true);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

   
//        var result = await _useCase.Execute(ownerId, tenantId);

     
//        Assert.False(result.Success);
//        Assert.Equal("Los roles no son válidos para esta operación", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenTenantIsNotInquilino_ReturnsFailure()
//    {
       
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false);

//        var tenant = new User
//        {
//            Id = tenantId,
//            Name = "Maria",
//            LastName = "Garcia",
//            Mail = "maria@test.com",
//            HasPermission = true,
//            Role = new Role { Id = 1, Description = "Propietario" },
//            Role_id = 1
//        };

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);


//        var result = await _useCase.Execute(ownerId, tenantId);

   
//        Assert.False(result.Success);
//        Assert.Equal("Los roles no son válidos para esta operación", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenTenantDoesNotHavePermission_ReturnsFailure()
//    {
    
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false);
//        var tenant = CreateTenant(tenantId, hasPermission: false); 

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        // Act
//        var result = await _useCase.Execute(ownerId, tenantId);

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal("El inquilino no tiene permisos activos para revocar", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenAllValidationsPass_RevokesPermissionsSuccessfully()
//    {
//        // Arrange
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false); 
//        var tenant = CreateTenant(tenantId, hasPermission: true); 

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
//        Assert.Equal("Permisos revocados exitosamente. El inquilino debe iniciar sesión nuevamente.", result.Message);
//        Assert.Equal(ownerId, result.OwnerId);
//        Assert.Equal(tenantId, result.TenantId);
//        Assert.True(result.OwnerHasPermission);
//        Assert.False(result.TenantHasPermission);

//        Assert.True(owner.HasPermission);
//        Assert.False(tenant.HasPermission);

//        _userRepositoryMock.Verify(x => x.Update(owner), Times.Once);
//        _userRepositoryMock.Verify(x => x.Update(tenant), Times.Once);
//        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllByUserId(tenantId), Times.Once);
//    }

//    [Fact]
//    public async Task Execute_WhenRevokingAlreadyRevokedPermissions_ReturnsFailure()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: true); 
//        var tenant = CreateTenant(tenantId, hasPermission: false);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.False(result.Success);
//        Assert.Equal("El inquilino no tiene permisos activos para revocar", result.Message);
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
//        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllByUserId(It.IsAny<int>()), Times.Never);
//    }

//    [Fact]
//    public async Task Execute_WhenRevokingPermissions_InvalidatesAllTenantTokens()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false);
//        var tenant = CreateTenant(tenantId, hasPermission: true);

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

//        _refreshTokenRepositoryMock.Verify(
//            x => x.RevokeAllByUserId(tenantId),
//            Times.Once,
//            "Debería invalidar todos los tokens del inquilino");

//        _refreshTokenRepositoryMock.Verify(
//            x => x.RevokeAllByUserId(ownerId),
//            Times.Never,
//            "NO debería invalidar los tokens del propietario");
//    }

//    [Fact]
//    public async Task Execute_WhenBothUsersHaveSameRoles_ReturnsFailure()
//    {
//        int ownerId = 1;
//        int tenantId = 2;

//        var owner = CreateOwner(ownerId, hasPermission: false);
//        var tenant = CreateOwner(tenantId, hasPermission: true); 

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(ownerId))
//            .ReturnsAsync(owner);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdWithRole(tenantId))
//            .ReturnsAsync(tenant);

//        var result = await _useCase.Execute(ownerId, tenantId);

//        Assert.False(result.Success);
//        Assert.Equal("Los roles no son válidos para esta operación", result.Message);
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
//            Role_id = 1
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
//            Role_id = 2
//        };
//    }
//}