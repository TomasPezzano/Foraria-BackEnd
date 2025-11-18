using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;

public class CreateExpenseDetailTests
{
    private readonly Mock<IExpenseDetailRepository> _expenseDetailRepositoryMock = new();
    private readonly Mock<IGetAllResidencesByConsortiumWithOwner> _getResidencesMock = new();
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock = new();

    private CreateExpenseDetail CreateUseCase()
    {
        return new CreateExpenseDetail(
            _expenseDetailRepositoryMock.Object,
            _getResidencesMock.Object,
            _residenceRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenExpenseIsNull()
    {
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            useCase.ExecuteAsync(null)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenExpenseIdIsInvalid()
    {
        var expense = new Expense { Id = 0, ConsortiumId = 1, TotalAmount = 1000 };
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenConsortiumIdIsInvalid()
    {
        var expense = new Expense { Id = 1, ConsortiumId = 0, TotalAmount = 1000 };
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenTotalAmountIsZero()
    {
        var expense = new Expense { Id = 1, ConsortiumId = 1, TotalAmount = 0 };
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidencesIsNull()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000,
            CreatedAt = DateTime.Now
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync())
            .ReturnsAsync((IEnumerable<Residence>)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFound_WhenResidencesIsEmpty()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000,
            CreatedAt = DateTime.Now
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync())
            .ReturnsAsync(new List<Residence>());

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidenceHasInvalidCoeficient()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000,
            CreatedAt = DateTime.Now
        };

        var residences = new List<Residence>
        {
            new Residence { Id = 5, Coeficient = 0 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(residences);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenDetailCreationFails()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000,
            CreatedAt = DateTime.Now
        };

        var residences = new List<Residence>
        {
            new Residence { Id = 5, Coeficient = 0.2 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync()).ReturnsAsync(residences);

        _residenceRepositoryMock
            .Setup(x => x.GetInvoicesByResidenceIdAsync(5, expense.CreatedAt))
            .ReturnsAsync(new List<Invoice>());

        _expenseDetailRepositoryMock.Setup(x =>
            x.AddExpenseDetailAsync(It.IsAny<ExpenseDetailByResidence>())
        ).ReturnsAsync((ExpenseDetailByResidence)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(expense));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateExpenseDetailsSuccessfully()
    {
        var expense = new Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000,
            CreatedAt = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc)

        };

        var residences = new List<Residence>
    {
        new Residence { Id = 1, Coeficient = 0.5, Expenses = new List<Expense>() },
        new Residence { Id = 2, Coeficient = 0.5, Expenses = new List<Expense>() }
    };

        _getResidencesMock.Setup(x => x.ExecuteAsync())
            .ReturnsAsync(residences);

        _residenceRepositoryMock
            .Setup(x => x.GetInvoicesByResidenceIdAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Invoice>());

        
        _expenseDetailRepositoryMock
            .Setup(x => x.AddExpenseDetailAsync(It.IsAny<ExpenseDetailByResidence>()))
            .ReturnsAsync((ExpenseDetailByResidence detail) =>
            {
                detail.Id = 99;
                detail.Expenses ??= new List<Expense>(); 
                return detail;
            });

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(expense);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        _expenseDetailRepositoryMock.Verify(
            x => x.AddExpenseDetailAsync(It.IsAny<ExpenseDetailByResidence>()),
            Times.Exactly(2)
        );
    }
}
