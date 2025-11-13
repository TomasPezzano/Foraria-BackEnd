using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit;
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

    // ============================================================
    // 1) Expense nulo
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenExpenseIsNull()
    {
        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            useCase.ExecuteAsync(null)
        );
    }

    // ============================================================
    // 2) Expense.Id inválido
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenExpenseIdIsInvalid()
    {
        var expense = new global::ForariaDomain.Expense
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

    // ============================================================
    // 3) Expense.ConsortiumId inválido
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenConsortiumIdIsInvalid()
    {
        var expense = new global::ForariaDomain.Expense
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

    // ============================================================
    // 4) TotalAmount <= 0
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenTotalAmountIsZeroOrNegative()
    {
        var expense = new global::ForariaDomain.Expense
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

    // ============================================================
    // 5) Residencias = null
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidencesIsNull()
    {
        var expense = new global::ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync((IEnumerable<global::ForariaDomain.Residence>)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    // ============================================================
    // 6) Residencias vacías
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFound_WhenNoResidencesFound()
    {
        var expense = new global::ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(new List<global::ForariaDomain.Residence>());

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    // ============================================================
    // 7) Residencia con coeficiente inválido
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenResidenceHasInvalidCoeficient()
    {
        var expense = new global::ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<global::ForariaDomain.Residence>
        {
            new global::ForariaDomain.Residence { Id = 5, Coeficient = 0 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );

        Assert.Contains("coeficiente inválido", ex.Message);
    }

    // ============================================================
    // 8) Error creando un detalle (AddExpenseDetailAsync retorna null)
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenDetailCreationFails()
    {
        var expense = new global::ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<global::ForariaDomain.Residence>
        {
            new global::ForariaDomain.Residence { Id = 5, Coeficient = 0.1 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        _expenseDetailRepositoryMock.Setup(x =>
            x.AddExpenseDetailAsync(It.IsAny<global::ForariaDomain.ExpenseDetailByResidence>())
        ).ReturnsAsync((global::ForariaDomain.ExpenseDetailByResidence)null);

        var useCase = CreateUseCase();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(expense)
        );
    }

    // ============================================================
    // 9) Ejecución exitosa
    // ============================================================
    [Fact]
    public async Task ExecuteAsync_ShouldCreateExpenseDetailsSuccessfully()
    {
        var expense = new global::ForariaDomain.Expense
        {
            Id = 1,
            ConsortiumId = 1,
            TotalAmount = 1000
        };

        var residences = new List<global::ForariaDomain.Residence>
        {
            new global::ForariaDomain.Residence { Id = 1, Coeficient = 0.5 },
            new global::ForariaDomain.Residence { Id = 2, Coeficient = 0.5 }
        };

        _getResidencesMock.Setup(x => x.ExecuteAsync(1))
            .ReturnsAsync(residences);

        _expenseDetailRepositoryMock.Setup(x =>
            x.AddExpenseDetailAsync(It.IsAny<global::ForariaDomain.ExpenseDetailByResidence>())
        ).ReturnsAsync((global::ForariaDomain.ExpenseDetailByResidence detail) =>
        {
            detail.Id = new Random().Next(100, 999);
            return detail;
        });

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(expense);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Verificar que se creó un detalle por cada residencia
        _expenseDetailRepositoryMock.Verify(
            x => x.AddExpenseDetailAsync(It.IsAny<global::ForariaDomain.ExpenseDetailByResidence>()),
            Times.Exactly(2)
        );
    }
}
