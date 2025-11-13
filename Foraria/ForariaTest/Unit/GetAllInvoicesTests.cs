using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using Xunit;

namespace ForariaTest.Unit;

public class GetAllInvoicesTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetAllInvoices _useCase;

    public GetAllInvoicesTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _useCase = new GetAllInvoices(_repositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnInvoices_WhenRepositoryReturnsData()
    {
        // Arrange
        var expectedInvoices = new List<Invoice>
        {
            new Invoice { Id = 1, Amount = 100 },
            new Invoice { Id = 2, Amount = 200 }
        };

        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ReturnsAsync(expectedInvoices);

        // Act
        var result = await _useCase.Execute();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Id == 1);
        Assert.Contains(result, i => i.Id == 2);

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyCollection_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _useCase.Execute();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _useCase.Execute());

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }
}
