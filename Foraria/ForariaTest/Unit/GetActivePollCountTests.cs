using Moq;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit;

public class GetActivePollCountTests
{
    private readonly Mock<IPollRepository> _pollRepoMock = new();

    private GetActivePollCount CreateUseCase()
    {
        return new GetActivePollCount(_pollRepoMock.Object);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoActivePolls()
    {
        _pollRepoMock
            .Setup(x => x.GetActivePolls(1, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<global::ForariaDomain.Poll>());

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(1);

        Assert.Equal(0, result);

        _pollRepoMock.Verify(
            x => x.GetActivePolls(1, It.IsAny<DateTime>()),
            Times.Once
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnCorrectCount_WhenPollsExist()
    {
        var polls = new List<global::ForariaDomain.Poll>
        {
            new global::ForariaDomain.Poll(),
            new global::ForariaDomain.Poll(),
            new global::ForariaDomain.Poll()
        };

        _pollRepoMock
            .Setup(x => x.GetActivePolls(5, It.IsAny<DateTime>()))
            .ReturnsAsync(polls);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(5);

        Assert.Equal(3, result);

        _pollRepoMock.Verify(
            x => x.GetActivePolls(5, It.IsAny<DateTime>()),
            Times.Once
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldUseProvidedDate_WhenDateTimeIsProvided()
    {
        var fixedDate = new DateTime(2024, 5, 1);

        _pollRepoMock
            .Setup(x => x.GetActivePolls(2, fixedDate))
            .ReturnsAsync(new List<global::ForariaDomain.Poll>());

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(2, fixedDate);

        Assert.Equal(0, result);

        _pollRepoMock.Verify(
            x => x.GetActivePolls(2, fixedDate),
            Times.Once
        );
    }
}
