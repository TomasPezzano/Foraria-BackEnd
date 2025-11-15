using Moq;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Reserves;
public class CreateReserveTests
{
    private readonly Mock<IReserveRepository> _reserveRepositoryMock = new();

    private CreateReserve CreateUseCase()
    {
        return new CreateReserve(_reserveRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldSetDeletedAtAndDate_AndCallRepositoryAdd()
    {
        // Arrange
        var reserve = new ForariaDomain.Reserve
        {
            CreatedAt = DateTime.UtcNow
        };

        _reserveRepositoryMock
            .Setup(x => x.Add(It.IsAny<ForariaDomain.Reserve>()))
            .Returns(Task.CompletedTask);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.Execute(reserve);

        // Assert
        Assert.NotNull(result);

        // Verifica que Date haya sido seteado (solo que no sea default)
        Assert.True(result.Date > DateTime.MinValue);

        // Verifica DeletedAt = CreatedAt + 1 hora
        Assert.Equal(reserve.CreatedAt.AddHours(1), result.DeletedAt);

        // Verifica que Add se llamó una vez
        _reserveRepositoryMock.Verify(
            x => x.Add(It.IsAny<ForariaDomain.Reserve>()),
            Times.Once
        );
    }
}
