using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Reserves;

public class GetAllReserveTests
{
    private readonly Mock<IReserveRepository> _reserveRepositoryMock;
    private readonly GetAllReserve _useCase;

    public GetAllReserveTests()
    {
        _reserveRepositoryMock = new Mock<IReserveRepository>();
        _useCase = new GetAllReserve(_reserveRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnReserves_WhenRepositoryReturnsData()
    {
        // Arrange
        int consortiumId = 1;
        var expectedReserves = new List<Reserve>
        {
            new Reserve { Id = 1, Description = "Reserva 1" },
            new Reserve { Id = 2, Description = "Reserva 2" }
        };

        _reserveRepositoryMock
            .Setup(r => r.GetAllInConsortium(consortiumId))
            .ReturnsAsync(expectedReserves);

        // Act
        var result = await _useCase.Execute(consortiumId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == 1);
        Assert.Contains(result, r => r.Id == 2);

        _reserveRepositoryMock.Verify(r => r.GetAllInConsortium(consortiumId), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        int consortiumId = 1;

        _reserveRepositoryMock
            .Setup(r => r.GetAllInConsortium(consortiumId))
            .ReturnsAsync(new List<Reserve>());

        // Act
        var result = await _useCase.Execute(consortiumId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _reserveRepositoryMock.Verify(r => r.GetAllInConsortium(consortiumId), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        int consortiumId = 1;

        _reserveRepositoryMock
            .Setup(r => r.GetAllInConsortium(consortiumId))
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.Execute(consortiumId));

        _reserveRepositoryMock.Verify(r => r.GetAllInConsortium(consortiumId), Times.Once);
    }
}
