using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Residences;

public class GetAllResidencesByConsortiumTests
{
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock;
    private readonly GetAllResidencesByConsortium _useCase;

    public GetAllResidencesByConsortiumTests()
    {
        _residenceRepositoryMock = new Mock<IResidenceRepository>();
        _useCase = new GetAllResidencesByConsortium(_residenceRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenConsortiumHasNoResidences()
    {
        // Arrange
        _residenceRepositoryMock
            .Setup(r => r.GetResidencesAsync())
            .ReturnsAsync(new List<Residence>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("El consorcio no tiene residencias asignadas", result.Message);
        Assert.Empty(result.Residences);

        _residenceRepositoryMock.Verify(r => r.GetResidencesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResidences_WhenConsortiumHasResidences()
    {
        // Arrange
        var expectedResidences = new List<Residence>
        {
            new Residence { Id = 1, Number = 101 },
            new Residence { Id = 2, Number = 102 }
        };

        _residenceRepositoryMock
            .Setup(r => r.GetResidencesAsync())
            .ReturnsAsync(expectedResidences);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Residencias obtenidas exitosamente", result.Message);
        Assert.Equal(2, result.Residences.Count);
        Assert.Contains(result.Residences, r => r.Id == 1);
        Assert.Contains(result.Residences, r => r.Id == 2);

        _residenceRepositoryMock.Verify(r => r.GetResidencesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _residenceRepositoryMock
            .Setup(r => r.GetResidencesAsync())
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync());

        _residenceRepositoryMock.Verify(r => r.GetResidencesAsync(), Times.Once);
    }
}