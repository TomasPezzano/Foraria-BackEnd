using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.Expenses;
public class CreateExpenseDetailTests
{
    private readonly Mock<IExpenseDetailRepository> _expenseDetailRepositoryMock = new();
    private readonly Mock<IGetAllResidencesByConsortiumWithOwner> _getResidencesMock = new();

    private CreateExpenseDetail CreateUseCase()
    {
        return new CreateExpenseDetail(
            _expenseDetailRepositoryMock.Object,
            _getResidencesMock.Object
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
        var expense = new ForariaDomain.Expense
        {
            Id = 0,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenConsortiumIdIsInvalid()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 0,
            TotalAmount = 1000
        };

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenTotalAmountIsZeroOrNegative()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 0
        };

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidencesIsNull()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync((IEnumerable<ForariaDomain.Residence>)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFound_WhenNoResidencesFound()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(new List<ForariaDomain.Residence>());

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidenceHasInvalidCoeficient()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<ForariaDomain.Residence>
        {
            new ForariaDomain.Residence { Id = 5, Coeficient = 0 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );

        Assert.Contains("coeficiente inválido", ex.Message);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenDetailCreationFails()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<ForariaDomain.Residence>
        {
            new ForariaDomain.Residence { Id = 5, Coeficient = 0.1 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        _expenseDetailRepositoryMock.Setup(x =>
            x.AddExpenseDetailAsync(It.IsAny<ForariaDomain.ExpenseDetailByResidence>())
        ).ReturnsAsync((ForariaDomain.ExpenseDetailByResidence)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldCreateExpenseDetailsSuccessfully()
    {
        var expense = new ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<ForariaDomain.Residence>
        {
            new ForariaDomain.Residence { Id = 1, Coeficient = 0.5 },
            new ForariaDomain.Residence { Id = 2, Coeficient = 0.5 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        _expenseDetailRepositoryMock.Setup(x =>
            x.AddExpenseDetailAsync(It.IsAny<ForariaDomain.ExpenseDetailByResidence>())
        ).ReturnsAsync((ForariaDomain.ExpenseDetailByResidence detail) =>
        {
            detail.Id = new Random().Next(100, 999);
            return detail;
        });

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(expense);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        _expenseDetailRepositoryMock.Verify(
            x => x.AddExpenseDetailAsync(It.IsAny<ForariaDomain.ExpenseDetailByResidence>()),
            Times.Exactly(2)
        );
    }
}
