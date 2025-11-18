using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaTest.Unit.Helpers;
using Moq;

namespace ForariaTest.Unit;

public class SelectConsortiumTests
{
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly SelectConsortium _selectConsortium;

    public SelectConsortiumTests()
    {
        _consortiumRepositoryMock = new Mock<IConsortiumRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _selectConsortium = new SelectConsortium(
            _consortiumRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    #region Happy Path Tests

    [Fact]
    public async Task Execute_WithValidAdministratorAndConsortium_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            name: "Carlos",
            lastName: "Pérez",
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Sur"
        );

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 1, 5, 8 });

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _selectConsortium.Execute(userId, consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Consorcio 'Torre Sur' seleccionado correctamente.", result.Message);
        Assert.NotNull(result.Consortium);
        Assert.Equal(consortiumId, result.Consortium.Id);
        Assert.Equal("Torre Sur", result.Consortium.Name);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(consortiumId),
            Times.Once
        );
        _userRepositoryMock.Verify(
            x => x.GetConsortiumIdsByUserId(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_WithValidConsejoUser_ReturnsSuccess()
    {
        // Arrange
        var userId = 10;
        var consortiumId = 3;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            name: "María",
            lastName: "González",
            roleDescription: "Consejo",
            roleId: 1
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Complejo Norte"
        );

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 3 });

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _selectConsortium.Execute(userId, consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Complejo Norte", result.Message);
        Assert.Equal("Consejo", user.Role.Description);
    }

    #endregion

    #region Exception Tests - Consorcio No Existe

    [Fact]
    public async Task Execute_WithNonExistentConsortium_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 999;

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync((Consortium?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Contains("no existe", exception.Message);
        Assert.Contains(consortiumId.ToString(), exception.Message);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(consortiumId),
            Times.Once
        );
        _userRepositoryMock.Verify(
            x => x.GetConsortiumIdsByUserId(It.IsAny<int>()),
            Times.Never
        );
    }

    #endregion

    #region Exception Tests - Usuario Sin Consorcios

    [Fact]
    public async Task Execute_WithUserWithoutConsortiums_ThrowsBusinessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var consortium = TestDataBuilder.CreateConsortium(id: consortiumId);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int>());  // Usuario sin consorcios

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Equal("No tienes consorcios asignados.", exception.Message);

        _userRepositoryMock.Verify(
            x => x.GetByIdWithoutFilters(It.IsAny<int>()),
            Times.Never  // No debe llegar a verificar el rol
        );
    }

    #endregion

    #region Exception Tests - Sin Acceso al Consorcio

    [Fact]
    public async Task Execute_WithUserWithoutAccessToConsortium_ThrowsForbiddenAccessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 10;
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Oeste"
        );

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 1, 5, 8 });  // Usuario tiene acceso a 1, 5, 8 pero NO a 10

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Contains("No tienes acceso al consorcio", exception.Message);
        Assert.Contains("Torre Oeste", exception.Message);
        Assert.Contains("1, 5, 8", exception.Message);
    }

    #endregion

    #region Exception Tests - Rol Inválido

    [Fact]
    public async Task Execute_WithPropietarioRole_ThrowsForbiddenAccessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            roleDescription: "Propietario",
            roleId: 3
        );
        var consortium = TestDataBuilder.CreateConsortium(id: consortiumId);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 5 });

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Contains("Solo usuarios con rol Administrador o Consejo", exception.Message);
    }

    [Fact]
    public async Task Execute_WithInquilinoRole_ThrowsForbiddenAccessException()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            roleDescription: "Inquilino",
            roleId: 4
        );
        var consortium = TestDataBuilder.CreateConsortium(id: consortiumId);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 5 });

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Contains("Solo usuarios con rol Administrador o Consejo", exception.Message);
    }

    #endregion

    #region Exception Tests - Usuario No Existe

    [Fact]
    public async Task Execute_WithNonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 999;
        var consortiumId = 5;
        var consortium = TestDataBuilder.CreateConsortium(id: consortiumId);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 5 });

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _selectConsortium.Execute(userId, consortiumId)
        );

        Assert.Equal("Usuario no encontrado.", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Execute_WithMultipleConsortiumsAccess_SelectsCorrectOne()
    {
        // Arrange
        var userId = 1;
        var consortiumId = 5;
        var user = TestDataBuilder.CreateUser(
            id: userId,
            roleDescription: "Administrador"
        );
        var consortium = TestDataBuilder.CreateConsortium(
            id: consortiumId,
            name: "Torre Media"
        );

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(new List<int> { 1, 3, 5, 8, 10 });  // Múltiples consorcios

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(consortiumId))
            .ReturnsAsync(consortium);

        _userRepositoryMock
            .Setup(x => x.GetByIdWithoutFilters(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _selectConsortium.Execute(userId, consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(consortiumId, result.Consortium.Id);
    }

    #endregion
}
