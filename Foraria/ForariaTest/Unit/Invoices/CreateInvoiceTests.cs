using Moq;
using Xunit;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.Invoices;

public class CreateInvoiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock = new();

    private CreateInvoice CreateUseCase()
    {
        return new CreateInvoice(_invoiceRepoMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldCallRepositoryAndReturnCreatedInvoice()
    {
        var invoice = new ForariaDomain.Invoice
        {
            Id = 0,
            Amount = 3000,
            Description = "Factura de prueba"
        };

        var createdInvoice = new ForariaDomain.Invoice
        {
            Id = 10,
            Amount = 3000,
            Description = "Factura de prueba"
        };

        _invoiceRepoMock
            .Setup(x => x.CreateInvoice(invoice))
            .ReturnsAsync(createdInvoice);

        var useCase = CreateUseCase();

        var result = await useCase.Execute(invoice);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(3000, result.Amount);
        Assert.Equal("Factura de prueba", result.Description);

        _invoiceRepoMock.Verify(
            x => x.CreateInvoice(invoice),
            Times.Once
        );
    }
}
