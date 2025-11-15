using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using Xunit;

namespace ForariaTest.Unit.Invoices;

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
        var expectedInvoices = new List<Invoice>
        {
            new Invoice { Id = 1, Amount = 100 },
            new Invoice { Id = 2, Amount = 200 }
        };

        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ReturnsAsync(expectedInvoices);

        var result = await _useCase.Execute();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Id == 1);
        Assert.Contains(result, i => i.Id == 2);

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyCollection_WhenRepositoryReturnsEmpty()
    {
        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ReturnsAsync(new List<Invoice>());

        var result = await _useCase.Execute();

        Assert.NotNull(result);
        Assert.Empty(result);

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenRepositoryThrows()
    {
        _repositoryMock
            .Setup(r => r.GetAllInvoices())
            .ThrowsAsync(new Exception("DB Error"));

        await Assert.ThrowsAsync<Exception>(() => _useCase.Execute());

        _repositoryMock.Verify(r => r.GetAllInvoices(), Times.Once);
    }
}
