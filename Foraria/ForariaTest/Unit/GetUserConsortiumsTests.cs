using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaTest.Unit.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit;

public class GetUserConsortiumsTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock;
    private readonly GetUserConsortiums _getUserConsortiums;

    public GetUserConsortiumsTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _consortiumRepositoryMock = new Mock<IConsortiumRepository>();
        _getUserConsortiums = new GetUserConsortiums(
            _userRepositoryMock.Object,
            _consortiumRepositoryMock.Object
        );
    }

    #region Happy Path Tests

    [Fact]
    public async Task Execute_WithAdminHavingMultipleConsortiums_ReturnsAllConsortiums()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = new List<int> { 1, 5, 8 };
        var consortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(1, "Torre Norte", "Edificio en Palermo"),
            TestDataBuilder.CreateConsortium(5, "Torre Sur", "Edificio en Recoleta"),
            TestDataBuilder.CreateConsortium(8, "Complejo Los Pinos", "Barrio cerrado")
        };

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        foreach (var consortium in consortiums)
        {
            _consortiumRepositoryMock
                .Setup(x => x.FindByIdWithoutFilters(consortium.Id))
                .ReturnsAsync(consortium);
        }

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.Id == 1 && c.Name == "Torre Norte");
        Assert.Contains(result, c => c.Id == 5 && c.Name == "Torre Sur");
        Assert.Contains(result, c => c.Id == 8 && c.Name == "Complejo Los Pinos");

        _userRepositoryMock.Verify(
            x => x.GetConsortiumIdsByUserId(userId),
            Times.Once
        );
        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Exactly(3)
        );
    }

    [Fact]
    public async Task Execute_WithUserHavingSingleConsortium_ReturnsSingleConsortium()
    {
        // Arrange
        var userId = 10;
        var consortiumIds = new List<int> { 3 };
        var consortium = TestDataBuilder.CreateConsortium(
            3,
            "Único Consorcio",
            "El único consorcio del usuario"
        );

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(3))
            .ReturnsAsync(consortium);

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Id);
        Assert.Equal("Único Consorcio", result[0].Name);
    }

    [Fact]
    public async Task Execute_ReturnsConsortiumsOrderedByName()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = new List<int> { 5, 1, 8 };  // Desordenados
        var consortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(5, "Zebra Building"),
            TestDataBuilder.CreateConsortium(1, "Alpha Tower"),
            TestDataBuilder.CreateConsortium(8, "Mega Complex")
        };

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        foreach (var consortium in consortiums)
        {
            _consortiumRepositoryMock
                .Setup(x => x.FindByIdWithoutFilters(consortium.Id))
                .ReturnsAsync(consortium);
        }

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alpha Tower", result[0].Name);      // A primero
        Assert.Equal("Mega Complex", result[1].Name);     // M segundo
        Assert.Equal("Zebra Building", result[2].Name);   // Z tercero
    }

    #endregion

    #region Edge Cases - Usuario Sin Consorcios

    [Fact]
    public async Task Execute_WithUserHavingNoConsortiums_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = new List<int>();  // Sin consorcios

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Empty(result);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Never  // No debe buscar consorcios si no tiene IDs
        );
    }

    #endregion

    #region Edge Cases - Consorcio No Encontrado

    [Fact]
    public async Task Execute_WithSomeConsortiumsNotFound_ReturnsOnlyFoundConsortiums()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = new List<int> { 1, 999, 5 };  // 999 no existe
        var existingConsortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(1, "Torre Norte"),
            TestDataBuilder.CreateConsortium(5, "Torre Sur")
        };

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(1))
            .ReturnsAsync(existingConsortiums[0]);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(999))
            .ReturnsAsync((Consortium?)null);  // No existe

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(5))
            .ReturnsAsync(existingConsortiums[1]);

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Equal(2, result.Count);  // Solo los 2 que existen
        Assert.Contains(result, c => c.Id == 1);
        Assert.Contains(result, c => c.Id == 5);
        Assert.DoesNotContain(result, c => c.Id == 999);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Exactly(3)  // Intentó buscar los 3
        );
    }

    [Fact]
    public async Task Execute_WithAllConsortiumsNotFound_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = new List<int> { 999, 888, 777 };  // Ninguno existe

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(It.IsAny<int>()))
            .ReturnsAsync((Consortium?)null);

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Empty(result);

        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Exactly(3)
        );
    }

    #endregion

    #region Different User Roles

    [Fact]
    public async Task Execute_WithPropietarioUser_ReturnsConsortiumFromResidence()
    {
        // Arrange
        var userId = 20;
        var consortiumIds = new List<int> { 3 };  // Un solo consorcio de su residencia
        var consortium = TestDataBuilder.CreateConsortium(
            3,
            "Edificio Residencia",
            "Consorcio del propietario"
        );

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        _consortiumRepositoryMock
            .Setup(x => x.FindByIdWithoutFilters(3))
            .ReturnsAsync(consortium);

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Id);
    }

    [Fact]
    public async Task Execute_WithConsejoUser_ReturnsConsortiumsFromResidences()
    {
        // Arrange
        var userId = 30;
        var consortiumIds = new List<int> { 2, 4 };  // Consejo puede tener múltiples residencias
        var consortiums = new List<Consortium>
        {
            TestDataBuilder.CreateConsortium(2, "Edificio A"),
            TestDataBuilder.CreateConsortium(4, "Edificio B")
        };

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        foreach (var consortium in consortiums)
        {
            _consortiumRepositoryMock
                .Setup(x => x.FindByIdWithoutFilters(consortium.Id))
                .ReturnsAsync(consortium);
        }

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Execute_WithManyConsortiums_CallsRepositoryCorrectNumberOfTimes()
    {
        // Arrange
        var userId = 1;
        var consortiumIds = Enumerable.Range(1, 10).ToList();  // 10 consorcios
        var consortiums = consortiumIds
            .Select(id => TestDataBuilder.CreateConsortium(id, $"Consorcio {id}"))
            .ToList();

        _userRepositoryMock
            .Setup(x => x.GetConsortiumIdsByUserId(userId))
            .Returns(consortiumIds);

        foreach (var consortium in consortiums)
        {
            _consortiumRepositoryMock
                .Setup(x => x.FindByIdWithoutFilters(consortium.Id))
                .ReturnsAsync(consortium);
        }

        // Act
        var result = await _getUserConsortiums.Execute(userId);

        // Assert
        Assert.Equal(10, result.Count);

        _userRepositoryMock.Verify(
            x => x.GetConsortiumIdsByUserId(userId),
            Times.Once
        );
        _consortiumRepositoryMock.Verify(
            x => x.FindByIdWithoutFilters(It.IsAny<int>()),
            Times.Exactly(10)
        );
    }

    #endregion
}
