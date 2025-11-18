using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaTest.Unit.Helpers;
using Moq;

namespace ForariaTest.Unit;

public class AssignConsortiumToAdminTests
{
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AssignConsortiumToAdmin _assignConsortiumToAdmin;

    public AssignConsortiumToAdminTests()
    {
        _consortiumRepositoryMock = new Mock<IConsortiumRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _assignConsortiumToAdmin = new AssignConsortiumToAdmin(
            _consortiumRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    #region Happy Path Tests

    [Fact]
    public async Task Execute_WithValidAdminAndConsortium_AssignsSuccessfully()
    {
        // Arrange
        var adminId = 1;
        var consortiumId = 5;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            name: "Carlos",
            lastName: "Pérez",
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Nueva",
            administratorId: null  // Sin admin asignado
        );
        var existingConsortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(1, "Torre Norte"),
            TestDataBuilder.CreateConsortium(5, "Torre Nueva")
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _consortiumRepositoryMock
            .Setup(x => x.HasAdministrator(consortiumId))
            .ReturnsAsync(false);

        _consortiumRepositoryMock
            .Setup(x => x.AssignAdministrator(consortiumId, adminId))
            .Returns(Task.CompletedTask);

        _consortiumRepositoryMock
            .Setup(x => x.GetConsortiumsByAdministrator(adminId))
            .ReturnsAsync(existingConsortiums);

        // Act
        var result = await _assignConsortiumToAdmin.Execute(adminId, consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Torre Nueva", result.Message);
        Assert.Contains("Carlos Pérez", result.Message);
        Assert.Equal(adminId, result.Admin.Id);
        Assert.Equal(2, result.Consortiums.Count);
        Assert.Contains(result.Consortiums, c => c.Id == consortiumId);

        _consortiumRepositoryMock.Verify(
            x => x.AssignAdministrator(consortiumId, adminId),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_AdminWithNoExistingConsortiums_AssignsFirstConsortium()
    {
        // Arrange
        var adminId = 10;
        var consortiumId = 3;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            name: "Nuevo",
            lastName: "Admin",
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Primer Consorcio"
        );
        var consortiumList = new List<Consortium> { consortium };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _consortiumRepositoryMock
            .Setup(x => x.HasAdministrator(consortiumId))
            .ReturnsAsync(false);

        _consortiumRepositoryMock
            .Setup(x => x.AssignAdministrator(consortiumId, adminId))
            .Returns(Task.CompletedTask);

        _consortiumRepositoryMock
            .Setup(x => x.GetConsortiumsByAdministrator(adminId))
            .ReturnsAsync(consortiumList);

        // Act
        var result = await _assignConsortiumToAdmin.Execute(adminId, consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Consortiums);
        Assert.Equal(consortiumId, result.Consortiums[0].Id);
    }

    #endregion

    #region Exception Tests - Usuario No Existe

    [Fact]
    public async Task Execute_WithNonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var adminId = 999;
        var consortiumId = 5;

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _assignConsortiumToAdmin.Execute(adminId, consortiumId)
        );

        Assert.Contains("no encontrado", exception.Message);
        Assert.Contains(adminId.ToString(), exception.Message);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Never
        );
    }

    #endregion

    #region Exception Tests - Usuario No Es Admin

    [Fact]
    public async Task Execute_WithPropietarioUser_ThrowsBusinessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            name: "Juan",
            lastName: "López",
            roleDescription: "Propietario",
            roleId: 3
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _assignConsortiumToAdmin.Execute(userId, consortiumId)
        );

        Assert.Contains("no tiene rol de Administrador", exception.Message);
        Assert.Contains("Juan López", exception.Message);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Execute_WithInquilinoUser_ThrowsBusinessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            roleDescription: "Inquilino",
            roleId: 4
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _assignConsortiumToAdmin.Execute(userId, consortiumId)
        );

        Assert.Contains("no tiene rol de Administrador", exception.Message);
    }

    [Fact]
    public async Task Execute_WithConsejoUser_ThrowsBusinessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            roleDescription: "Consejo",
            roleId: 1
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _assignConsortiumToAdmin.Execute(userId, consortiumId)
        );

        Assert.Contains("no tiene rol de Administrador", exception.Message);
    }

    #endregion

    #region Exception Tests - Consorcio No Existe

    [Fact]
    public async Task Execute_WithNonExistentConsortium_ThrowsNotFoundException()
    {
        // Arrange
        var adminId = 1;
        var consortiumId = 999;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            roleDescription: "Administrador"
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync((Consortium?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _assignConsortiumToAdmin.Execute(adminId, consortiumId)
        );

        Assert.Contains("no encontrado", exception.Message);
        Assert.Contains(consortiumId.ToString(), exception.Message);
    }

    #endregion

    #region Exception Tests - Consorcio Ya Tiene Admin

    [Fact]
    public async Task Execute_WithConsortiumAlreadyAssignedToSameAdmin_ThrowsBusinessException()
    {
        // Arrange
        var adminId = 1;
        var consortiumId = 5;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            name: "Carlos",
            lastName: "Pérez",
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Sur",
            administratorId: adminId  // Ya asignado al mismo admin
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _consortiumRepositoryMock
            .Setup(x => x.HasAdministrator(consortiumId))
            .ReturnsAsync(true);

        _consortiumRepositoryMock
            .Setup(x => x.IsAdministratorAssigned(consortiumId, adminId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _assignConsortiumToAdmin.Execute(adminId, consortiumId)
        );

        Assert.Contains("ya está asignado", exception.Message);
        Assert.Contains("Carlos Pérez", exception.Message);
        Assert.Contains("Torre Sur", exception.Message);

        _consortiumRepositoryMock.Verify(
            x => x.AssignAdministrator(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Execute_WithConsortiumAssignedToDifferentAdmin_ThrowsBusinessException()
    {
        // Arrange
        var adminId = 1;
        var otherAdminId = 2;
        var consortiumId = 5;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            name: "Carlos",
            lastName: "Pérez",
            roleDescription: "Administrador"
        );
        var otherAdmin = TestDataBuilder.CreateUser(
            id: otherAdminId,
            name: "María",
            lastName: "González",
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Oeste",
            administratorId: otherAdminId  // Asignado a otro admin
        );
        consortium.Administrator = otherAdmin;

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _consortiumRepositoryMock
            .Setup(x => x.HasAdministrator(consortiumId))
            .ReturnsAsync(true);

        _consortiumRepositoryMock
            .Setup(x => x.IsAdministratorAssigned(consortiumId, adminId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _assignConsortiumToAdmin.Execute(adminId, consortiumId)
        );

        Assert.Contains("ya tiene un administrador asignado", exception.Message);
        Assert.Contains("María González", exception.Message);
        Assert.Contains(otherAdminId.ToString(), exception.Message);

        _consortiumRepositoryMock.Verify(
            x => x.AssignAdministrator(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never
        );
    }

    #endregion

    #region Integration-Style Tests

    [Fact]
    public async Task Execute_AdminAssignsMultipleConsortiums_ReturnsAllConsortiums()
    {
        // Arrange
        var adminId = 1;
        var newConsortiumId = 10;
        var admin = TestDataBuilder.CreateUser(
            id: adminId,
            name: "Super",
            lastName: "Admin",
            roleDescription: "Administrador"
        );
        var newConsortium = TestDataBuilder.CreateConsortium(
            id: newConsortiumId,
            name: "Consorcio Nuevo"
        );
        var allConsortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(1, "Consorcio A"),
            TestDataBuilder.CreateConsortium(5, "Consorcio B"),
            TestDataBuilder.CreateConsortium(8, "Consorcio C"),
            TestDataBuilder.CreateConsortium(10, "Consorcio Nuevo")
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(adminId))
            .ReturnsAsync(admin);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(newConsortiumId))
            .ReturnsAsync(newConsortium);

        _consortiumRepositoryMock
            .Setup(x => x.HasAdministrator(newConsortiumId))
            .ReturnsAsync(false);

        _consortiumRepositoryMock
            .Setup(x => x.AssignAdministrator(newConsortiumId, adminId))
            .Returns(Task.CompletedTask);

        _consortiumRepositoryMock
            .Setup(x => x.GetConsortiumsByAdministrator(adminId))
            .ReturnsAsync(allConsortiums);

        // Act
        var result = await _assignConsortiumToAdmin.Execute(adminId, newConsortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(4, result.Consortiums.Count);
        Assert.Contains(result.Consortiums, c => c.Id == 1);
        Assert.Contains(result.Consortiums, c => c.Id == 5);
        Assert.Contains(result.Consortiums, c => c.Id == 8);
        Assert.Contains(result.Consortiums, c => c.Id == 10);
    }

    #endregion
}
