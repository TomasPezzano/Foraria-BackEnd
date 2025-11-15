using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;

namespace ForariaTest.Unit.Expenses;

public class CreateExpenseTests
{
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<IGetAllInvoicesByMonthAndConsortium> _getAllInvoicesMock = new();
    private readonly Mock<IGetConsortiumById> _getConsortiumMock = new();
    private readonly Mock<IGetAllResidencesByConsortiumWithOwner> _getResidencesMock = new();
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock = new();

    private CreateExpense CreateUseCase()
    {
        return new CreateExpense(
            _expenseRepositoryMock.Object,
            _getAllInvoicesMock.Object,
            _getConsortiumMock.Object,
            _invoiceRepositoryMock.Object,
            _getResidencesMock.Object,
            _residenceRepositoryMock.Object
        );
    }


    [Theory]
    [InlineData("2025/10")]
    [InlineData("10-2025")]
    [InlineData("2025-13")]
    [InlineData("abc-def")]
    [InlineData("")]
    public async Task ExecuteAsync_ShouldThrowFormatException_WhenDateIsInvalid(string invalidDate)
    {
        var useCase = CreateUseCase();

        var exception = await Assert.ThrowsAsync<FormatException>(() =>
            useCase.ExecuteAsync(1, invalidDate)
        );

        Assert.StartsWith("El formato de la fecha es inválido", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenConsortiumDoesNotExist()
    {
        _getConsortiumMock.Setup(x => x.Execute(99))
            .ReturnsAsync((Consortium)null);

        var useCase = CreateUseCase();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(99, "2025-10")
        );

        Assert.Contains("99", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenNoInvoicesFound()
    {
        _getConsortiumMock.Setup(x => x.Execute(1))
            .ReturnsAsync(new Consortium { Id = 1 });

        _getAllInvoicesMock.Setup(x => x.Execute(It.IsAny<DateTime>(), 1))
            .ReturnsAsync(new List<Invoice>());

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(1, "2025-10")
        );

        Assert.StartsWith("No existen facturas registradas", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenTotalAmountIsZeroOrNegative()
    {
        var invoices = new List<Invoice>
    {
        new Invoice { Id = 1, Amount = 0 }
    };

        _getConsortiumMock.Setup(x => x.Execute(1))
            .ReturnsAsync(new Consortium { Id = 1 });

        _getAllInvoicesMock.Setup(x => x.Execute(It.IsAny<DateTime>(), 1))
            .ReturnsAsync(invoices);

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(new List<Residence>
            {
            new Residence { Id = 1, Expenses = new List<Expense>() }
            });

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(1, "2025-10")
        );

        Assert.Equal("El total de las facturas no puede ser cero o negativo.", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldCreateExpenseSuccessfully()
    {
        var consortium = new Consortium { Id = 1 };

        var invoices = new List<Invoice>
        {
            new Invoice { Id = 10, Amount = 500 },
            new Invoice { Id = 11, Amount = 250 }
        };

        var residences = new List<Residence>
        {
            new Residence { Id = 1, Expenses = new List<Expense>() },
            new Residence { Id = 2, Expenses = new List<Expense>() }
        };

        var expectedExpenseId = 123;

        _getConsortiumMock.Setup(x => x.Execute(1))
            .ReturnsAsync(consortium);

        _getAllInvoicesMock.Setup(x => x.Execute(It.IsAny<DateTime>(), 1))
            .ReturnsAsync(invoices);

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        _expenseRepositoryMock
            .Setup(x => x.AddExpenseAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) =>
            {
                e.Id = expectedExpenseId;
                return e;
            });

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(1, "2025-10");


        Assert.NotNull(result);
        Assert.Equal(expectedExpenseId, result.Id);
        Assert.Equal(750, result.TotalAmount);
        Assert.Equal(2, result.Invoices.Count);


        _invoiceRepositoryMock.Verify(
            x => x.UpdateInvoiceAsync(It.Is<Invoice>(i => i.ExpenseId == expectedExpenseId)),
            Times.Exactly(2)
        );


        _residenceRepositoryMock.Verify(
            x => x.UpdateExpense(It.IsAny<Residence>()),
            Times.Exactly(2)
        );


        Assert.All(residences, r =>
            Assert.Contains(result, r.Expenses)
        );

        _expenseRepositoryMock.Verify(
            x => x.AddExpenseAsync(It.IsAny<Expense>()),
            Times.Once
        );
    }
}
