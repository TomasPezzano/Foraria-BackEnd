using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;

namespace ForariaTest.Unit;

public class GetAllResidencesByConsortiumTests
{
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock;
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock;
    private readonly GetAllResidencesByConsortium _useCase;

    public GetAllResidencesByConsortiumTests()
    {
        _residenceRepositoryMock = new Mock<IResidenceRepository>();
        _consortiumRepositoryMock = new Mock<IConsortiumRepository>();
        _useCase = new GetAllResidencesByConsortium(_residenceRepositoryMock.Object, _consortiumRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenConsortiumIdIsInvalid()
    {
        // Arrange
        int consortiumId = 0;

        // Act
        var result = await _useCase.ExecuteAsync(consortiumId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El ID del consorcio debe ser mayor a 0", result.Message);
        Assert.Empty(result.Residences);

        _consortiumRepositoryMock.Verify(r => r.FindById(It.IsAny<int>()), Times.Never);
        _residenceRepositoryMock.Verify(r => r.GetResidenceByConsortiumIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenConsortiumDoesNotExist()
    {
        // Arrange
        int consortiumId = 5;

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync((Consortium?)null);

        // Act
        var result = await _useCase.ExecuteAsync(consortiumId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("El consorcio con ID 5 no existe", result.Message);
        Assert.Empty(result.Residences);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _residenceRepositoryMock.Verify(r => r.GetResidenceByConsortiumIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenConsortiumHasNoResidences()
    {
        // Arrange
        int consortiumId = 3;

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync(new Consortium { Id = consortiumId });

        _residenceRepositoryMock
            .Setup(r => r.GetResidenceByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(new List<Residence>());

        // Act
        var result = await _useCase.ExecuteAsync(consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("El consorcio no tiene residencias asignadas", result.Message);
        Assert.Empty(result.Residences);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _residenceRepositoryMock.Verify(r => r.GetResidenceByConsortiumIdAsync(consortiumId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResidences_WhenConsortiumHasResidences()
    {
        // Arrange
        int consortiumId = 7;

        var expectedResidences = new List<Residence>
        {
            new Residence { Id = 1, Number = 101 },
            new Residence { Id = 2, Number = 102 }
        };

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ReturnsAsync(new Consortium { Id = consortiumId });

        _residenceRepositoryMock
            .Setup(r => r.GetResidenceByConsortiumIdAsync(consortiumId))
            .ReturnsAsync(expectedResidences);

        // Act
        var result = await _useCase.ExecuteAsync(consortiumId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Residencias obtenidas exitosamente", result.Message);
        Assert.Equal(2, result.Residences.Count);
        Assert.Contains(result.Residences, r => r.Id == 1);
        Assert.Contains(result.Residences, r => r.Id == 2);

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _residenceRepositoryMock.Verify(r => r.GetResidenceByConsortiumIdAsync(consortiumId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        int consortiumId = 10;

        _consortiumRepositoryMock
            .Setup(r => r.FindById(consortiumId))
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(consortiumId));

        _consortiumRepositoryMock.Verify(r => r.FindById(consortiumId), Times.Once);
        _residenceRepositoryMock.Verify(r => r.GetResidenceByConsortiumIdAsync(It.IsAny<int>()), Times.Never);
    }
}
