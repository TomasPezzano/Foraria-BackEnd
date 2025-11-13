using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;


namespace ForariaTest.Unit;
public class GetAllInvoicesByMonthAndConsortiumTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetAllInvoicesByMonthAndConsortium _useCase;

    public GetAllInvoicesByMonthAndConsortiumTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _useCase = new GetAllInvoicesByMonthAndConsortium(_repositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnInvoices_WhenRepositoryReturnsData()
    {
        // Arrange
        var date = new DateTime(2024, 10, 1);
        var consortiumId = 3;

        var expectedInvoices = new List<Invoice>
        {
            new Invoice { Id = 1, Amount = 900 },
            new Invoice { Id = 2, Amount = 1200 }
        };

        _repositoryMock
            .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId))
            .ReturnsAsync(expectedInvoices);

        // Act
        var result = await _useCase.Execute(date, consortiumId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Id == 1);
        Assert.Contains(result, i => i.Id == 2);

        _repositoryMock.Verify(
            r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_ShouldReturnEmpty_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        var date = new DateTime(2024, 10, 1);
        var consortiumId = 3;

        _repositoryMock
            .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId))
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _useCase.Execute(date, consortiumId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _repositoryMock.Verify(
            r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var date = DateTime.Now;
        var consortiumId = 7;

        _repositoryMock
            .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId))
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.Execute(date, consortiumId));

        _repositoryMock.Verify(
            r => r.GetAllInvoicesByMonthAndConsortium(date, consortiumId),
            Times.Once
        );
    }
}
