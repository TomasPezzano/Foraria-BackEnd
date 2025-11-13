using Moq;
using Xunit;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit;

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
        // Arrange
        var invoice = new global::ForariaDomain.Invoice
        {
            Id = 0,
            Amount = 3000,
            Description = "Factura de prueba"
        };

        var createdInvoice = new global::ForariaDomain.Invoice
        {
            Id = 10,
            Amount = 3000,
            Description = "Factura de prueba"
        };

        _invoiceRepoMock
            .Setup(x => x.CreateInvoice(invoice))
            .ReturnsAsync(createdInvoice);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.Execute(invoice);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(3000, result.Amount);
        Assert.Equal("Factura de prueba", result.Description);

        // Verificar que CreateInvoice se llamó una vez con el invoice correcto
        _invoiceRepoMock.Verify(
            x => x.CreateInvoice(invoice),
            Times.Once
        );
    }
}
