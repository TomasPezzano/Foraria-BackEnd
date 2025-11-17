using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Services;

namespace ForariaTest.Unit.Expenses
{
    public class GetExpenseWithDtoTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetExpenseWithDto _useCase;

        public GetExpenseWithDtoTests()
        {
            _expenseRepositoryMock = new Mock<IExpenseRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            _tenantContextMock.Setup(t => t.GetCurrentConsortiumId()).Returns(1);

            _useCase = new GetExpenseWithDto(
                _expenseRepositoryMock.Object,
                _tenantContextMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowArgumentException_WhenConsortiumIdIsZeroOrLess()
        {
            _tenantContextMock.Setup(t => t.GetCurrentConsortiumId()).Returns(0);

            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync("2024-01"));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowArgumentException_WhenDateIsNullOrEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync("   "));
            await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(null));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenRepositoryReturnsNull()
        {
            _expenseRepositoryMock
                .Setup(r => r.GetExpenseByConsortiumAndMonthAsync("2024-01"))
                .ReturnsAsync((Expense)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _useCase.ExecuteAsync("2024-01"));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnExpense_WhenDataExists()
        {
            var fakeExpense = new Expense { Id = 123 };

            _expenseRepositoryMock
                .Setup(r => r.GetExpenseByConsortiumAndMonthAsync("2024-01"))
                .ReturnsAsync(fakeExpense);

            var result = await _useCase.ExecuteAsync("2024-01");

            Assert.NotNull(result);
            Assert.Equal(123, result.Id);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenUnexpectedExceptionOccurs()
        {
            _expenseRepositoryMock
                .Setup(r => r.GetExpenseByConsortiumAndMonthAsync("2024-01"))
                .ThrowsAsync(new Exception("DB failure"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _useCase.ExecuteAsync("2024-01")
            );

            Assert.Contains("Error al obtener la expensa", ex.Message);
            Assert.NotNull(ex.InnerException);
        }
    }
}
