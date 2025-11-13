using Moq;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;


namespace ForariaTest.Unit;

public class CreatePreferenceMPTests
{
    private readonly Mock<IExpenseDetailRepository> _expenseDetailRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IPaymentService> _paymentServiceMock = new();

    private CreatePreferenceMP CreateUseCase()
    {
        return new CreatePreferenceMP(
            _paymentRepositoryMock.Object,
            _paymentServiceMock.Object,
            _expenseDetailRepositoryMock.Object
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenExpenseDetailNotFound()
    {
        _expenseDetailRepositoryMock
            .Setup(x => x.GetExpenseDetailById(10))
            .ReturnsAsync((global::ForariaDomain.ExpenseDetailByResidence)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            useCase.ExecuteAsync(10, 5)
        );

        Assert.Equal("Expense no encontrada.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreatePreferenceAndSavePayment_WhenValidRequest()
    {
        // Arrange
        var expenseDetail = new global::ForariaDomain.ExpenseDetailByResidence
        {
            Id = 10,
            TotalAmount = 500,
            ResidenceId = 5
        };

        _expenseDetailRepositoryMock
            .Setup(x => x.GetExpenseDetailById(10))
            .ReturnsAsync(expenseDetail);

        _paymentServiceMock
            .Setup(x => x.CreatePreferenceAsync(500m, 10, 5))
            .ReturnsAsync(("pref-123", "initpoint-xyz"));

        _paymentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<global::ForariaDomain.Payment>()))
            .Returns(Task.CompletedTask);

        _paymentRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(10, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pref-123", result.PreferenceId);
        Assert.Equal("initpoint-xyz", result.InitPoint);

        // Verificar que se llamó al gateway correctamente
        _paymentServiceMock.Verify(
            x => x.CreatePreferenceAsync(500m, 10, 5),
            Times.Once
        );

        // Verificar que se creó un Payment con los datos correctos
        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.Is<global::ForariaDomain.Payment>(p =>
                p.PreferenceId == "pref-123"
                && p.Amount == 500m
                && p.ExpenseDetailByResidenceId == 10
                && p.ResidenceId == 5
                && p.Status == "pending"
            )),
            Times.Once
        );

        // Verificar SaveChangesAsync
        _paymentRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once
        );
    }


    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenPaymentGatewayFails()
    {
        var expenseDetail = new global::ForariaDomain.ExpenseDetailByResidence
        {
            Id = 10,
            TotalAmount = 500,
            ResidenceId = 5
        };

        _expenseDetailRepositoryMock
            .Setup(x => x.GetExpenseDetailById(10))
            .ReturnsAsync(expenseDetail);

        _paymentServiceMock
            .Setup(x => x.CreatePreferenceAsync(500m, 10, 5))
            .ThrowsAsync(new Exception("Error en MP"));

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            useCase.ExecuteAsync(10, 5)
        );

        Assert.Equal("Error en MP", ex.Message);
    }
}
