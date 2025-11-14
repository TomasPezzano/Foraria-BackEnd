using Moq;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Reserves;

public class GetActiveReserveCountTests
{
    private readonly Mock<IReserveRepository> _reserveRepoMock = new();

    private GetActiveReserveCount CreateUseCase()
    {
        return new GetActiveReserveCount(_reserveRepoMock.Object);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnZero_WhenNoActiveReservations()
    {
        _reserveRepoMock
            .Setup(x => x.GetActiveReservationsAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ForariaDomain.Reserve>());

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(1);

        Assert.Equal(0, result);

        _reserveRepoMock.Verify(
            x => x.GetActiveReservationsAsync(1, It.IsAny<DateTime>()),
            Times.Once
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnCorrectCount_WhenReservationsExist()
    {
        var reservations = new List<ForariaDomain.Reserve>
        {
            new ForariaDomain.Reserve(),
            new ForariaDomain.Reserve()
        };

        _reserveRepoMock
            .Setup(x => x.GetActiveReservationsAsync(5, It.IsAny<DateTime>()))
            .ReturnsAsync(reservations);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(5);

        Assert.Equal(2, result);

        _reserveRepoMock.Verify(
            x => x.GetActiveReservationsAsync(5, It.IsAny<DateTime>()),
            Times.Once
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldUseProvidedDate_WhenDateTimeProvided()
    {
        var fixedDate = new DateTime(2023, 10, 1);

        _reserveRepoMock
            .Setup(x => x.GetActiveReservationsAsync(3, fixedDate))
            .ReturnsAsync(new List<ForariaDomain.Reserve>());

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(3, fixedDate);

        Assert.Equal(0, result);

        _reserveRepoMock.Verify(
            x => x.GetActiveReservationsAsync(3, fixedDate),
            Times.Once
        );
    }
}
