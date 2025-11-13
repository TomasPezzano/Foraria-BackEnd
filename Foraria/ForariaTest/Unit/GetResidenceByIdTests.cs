using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit;

public class GetResidenceByIdTests
{
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock;
    private readonly GetResidenceById _useCase;

    public GetResidenceByIdTests()
    {
        _residenceRepositoryMock = new Mock<IResidenceRepository>();
        _useCase = new GetResidenceById(_residenceRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenIdIsInvalid()
    {
        // Arrange
        int invalidId = 0;

        // Act
        var result = await _useCase.Execute(invalidId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El ID de la residencia debe ser mayor a 0", result.Message);
        Assert.Null(result.Residence);

        _residenceRepositoryMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenResidenceNotFound()
    {
        // Arrange
        int residenceId = 5;

        _residenceRepositoryMock
            .Setup(r => r.GetById(residenceId))
            .ReturnsAsync((Residence?)null);

        // Act
        var result = await _useCase.Execute(residenceId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"No se encontró la residencia con ID {residenceId}", result.Message);
        Assert.Null(result.Residence);

        _residenceRepositoryMock.Verify(r => r.GetById(residenceId), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnResidence_WhenResidenceExists()
    {
        // Arrange
        int residenceId = 10;

        var expectedResidence = new Residence
        {
            Id = residenceId,
            Number = 12
        };

        _residenceRepositoryMock
            .Setup(r => r.GetById(residenceId))
            .ReturnsAsync(expectedResidence);

        // Act
        var result = await _useCase.Execute(residenceId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Residencia encontrada", result.Message);
        Assert.NotNull(result.Residence);
        Assert.Equal(expectedResidence, result.Residence);

        _residenceRepositoryMock.Verify(r => r.GetById(residenceId), Times.Once);
    }
}
