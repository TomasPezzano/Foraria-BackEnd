using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit;

public class GetAllPollsWithResultsTests
{
    private readonly Mock<IPollRepository> _pollRepositoryMock;
    private readonly GetAllPollsWithResults _useCase;

    public GetAllPollsWithResultsTests()
    {
        _pollRepositoryMock = new Mock<IPollRepository>();
        _useCase = new GetAllPollsWithResults(_pollRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPolls_WhenRepositoryReturnsData()
    {
        // Arrange
        var expectedPolls = new List<Poll>
        {
            new Poll { Id = 1, Title = "Encuesta 1" },
            new Poll { Id = 2, Title = "Encuesta 2" }
        };

        _pollRepositoryMock
            .Setup(r => r.GetAllPollsWithResultsAsync())
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Id == 1);
        Assert.Contains(result, p => p.Id == 2);

        _pollRepositoryMock.Verify(r => r.GetAllPollsWithResultsAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        _pollRepositoryMock
            .Setup(r => r.GetAllPollsWithResultsAsync())
            .ReturnsAsync(new List<Poll>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _pollRepositoryMock.Verify(r => r.GetAllPollsWithResultsAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _pollRepositoryMock
            .Setup(r => r.GetAllPollsWithResultsAsync())
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync());

        _pollRepositoryMock.Verify(r => r.GetAllPollsWithResultsAsync(), Times.Once);
    }
}
