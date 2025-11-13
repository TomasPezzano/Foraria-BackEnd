using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit
{
    public class GetAllExpensesTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepoMock = new();

        private GetAllExpenses CreateUseCase()
        {
            return new GetAllExpenses(_expenseRepoMock.Object);
        }


        [Fact]
        public async Task Execute_ShouldThrowInvalidOperation_WhenRepositoryReturnsNull()
        {
            _expenseRepoMock
                .Setup(x => x.GetAllExpenses())
                .ReturnsAsync((IEnumerable<global::ForariaDomain.Expense>)null);

            var useCase = CreateUseCase();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                useCase.Execute()
            );

            Assert.Equal("El repositorio devolvió un valor nulo al obtener las expensas.", ex.Message);
        }


        [Fact]
        public async Task Execute_ShouldThrowKeyNotFound_WhenExpensesListIsEmpty()
        {
            _expenseRepoMock
                .Setup(x => x.GetAllExpenses())
                .ReturnsAsync(new List<global::ForariaDomain.Expense>());

            var useCase = CreateUseCase();

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                useCase.Execute()
            );

            Assert.Equal("No se encontraron expensas registradas.", ex.Message);
        }


        [Fact]
        public async Task Execute_ShouldReturnExpenses_WhenListIsNotEmpty()
        {
            var expenses = new List<global::ForariaDomain.Expense>
            {
                new global::ForariaDomain.Expense { Id = 1, TotalAmount = 500 },
                new global::ForariaDomain.Expense { Id = 2, TotalAmount = 1000 }
            };

            _expenseRepoMock
                .Setup(x => x.GetAllExpenses())
                .ReturnsAsync(expenses);

            var useCase = CreateUseCase();

            var result = await useCase.Execute();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(1, result.First().Id);
            Assert.Equal(1000, result.Last().TotalAmount);

            _expenseRepoMock.Verify(x => x.GetAllExpenses(), Times.Once);
        }


        [Fact]
        public async Task Execute_ShouldWrapUnexpectedExceptionIntoInvalidOperation()
        {
            _expenseRepoMock
                .Setup(x => x.GetAllExpenses())
                .ThrowsAsync(new Exception("Fallo inesperado"));

            var useCase = CreateUseCase();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                useCase.Execute()
            );

            Assert.StartsWith("Ocurrió un error inesperado al obtener las expensas.", ex.Message);

            Assert.NotNull(ex.InnerException);
            Assert.Equal("Fallo inesperado", ex.InnerException.Message);
        }
    }
}
