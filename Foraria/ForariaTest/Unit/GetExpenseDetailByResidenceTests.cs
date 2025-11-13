using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using Xunit;

namespace ForariaTest.Unit;
public class GetExpenseDetailByResidenceTests
{
    private readonly Mock<IExpenseDetailRepository> _expenseDetailRepositoryMock;
    private readonly GetExpenseDetailByResidence _useCase;

    public GetExpenseDetailByResidenceTests()
    {
        _expenseDetailRepositoryMock = new Mock<IExpenseDetailRepository>();
        _useCase = new GetExpenseDetailByResidence(_expenseDetailRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
    {
        // Arrange
        int invalidId = 0;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(invalidId));

        Assert.StartsWith("El ID de la residencia no es válido.", ex.Message);
        Assert.Equal("id", ex.ParamName);

        _expenseDetailRepositoryMock.Verify(r => r.GetExpenseDetailByResidence(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        int id = 5;

        _expenseDetailRepositoryMock
            .Setup(r => r.GetExpenseDetailByResidence(id))
            .ReturnsAsync((ICollection<ExpenseDetailByResidence>)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(id));
        Assert.Equal("El repositorio devolvió un valor nulo al obtener los detalles de expensa.", ex.Message);

        _expenseDetailRepositoryMock.Verify(r => r.GetExpenseDetailByResidence(id), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenRepositoryReturnsEmptyList()
    {
        // Arrange
        int id = 3;

        _expenseDetailRepositoryMock
            .Setup(r => r.GetExpenseDetailByResidence(id))
            .ReturnsAsync(new List<ExpenseDetailByResidence>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecuteAsync(id));
        Assert.Equal($"No se encontraron detalles de expensa para la residencia con ID {id}.", ex.Message);

        _expenseDetailRepositoryMock.Verify(r => r.GetExpenseDetailByResidence(id), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnData_WhenRepositoryReturnsValidDetails()
    {
        // Arrange
        int id = 10;
        var details = new List<ExpenseDetailByResidence>
        {
            new ExpenseDetailByResidence { Id = 1 },
            new ExpenseDetailByResidence { Id = 2},
        };

        _expenseDetailRepositoryMock
            .Setup(r => r.GetExpenseDetailByResidence(id))
            .ReturnsAsync(details);

        // Act
        var result = await _useCase.ExecuteAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);

        _expenseDetailRepositoryMock.Verify(r => r.GetExpenseDetailByResidence(id), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWrapUnexpectedException_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        int id = 999;

        _expenseDetailRepositoryMock
            .Setup(r => r.GetExpenseDetailByResidence(id))
            .ThrowsAsync(new Exception("Error inesperado en DB"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(id));
        Assert.Equal("Ocurrió un error inesperado al obtener los detalles de expensa.", ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.Equal("Error inesperado en DB", ex.InnerException.Message);

        _expenseDetailRepositoryMock.Verify(r => r.GetExpenseDetailByResidence(id), Times.Once);
    }
}
