using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Invoices
{
    public class GetAllInvoicesByMonthAndConsortiumTests
    {
        private readonly Mock<IInvoiceRepository> _repositoryMock;
        private readonly GetAllInvoicesByMonthAndConsortium _useCase;

        public GetAllInvoicesByMonthAndConsortiumTests()
        {
            _repositoryMock = new Mock<IInvoiceRepository>();
            _useCase = new GetAllInvoicesByMonthAndConsortium(_repositoryMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnInvoices_WhenRepositoryReturnsData()
        {
            var date = new DateTime(2024, 10, 1);

            var expectedInvoices = new List<Invoice>
            {
                new Invoice { Id = 1, Amount = 900 },
                new Invoice { Id = 2, Amount = 1200 }
            };

            _repositoryMock
                .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date))
                .ReturnsAsync(expectedInvoices);

            var result = await _useCase.Execute(date);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.Id == 1);
            Assert.Contains(result, i => i.Id == 2);

            _repositoryMock.Verify(
                r => r.GetAllInvoicesByMonthAndConsortium(date),
                Times.Once
            );
        }

        [Fact]
        public async Task Execute_ShouldReturnEmpty_WhenRepositoryReturnsEmpty()
        {
            var date = new DateTime(2024, 10, 1);

            _repositoryMock
                .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date))
                .ReturnsAsync(new List<Invoice>());

            var result = await _useCase.Execute(date);

            Assert.NotNull(result);
            Assert.Empty(result);

            _repositoryMock.Verify(
                r => r.GetAllInvoicesByMonthAndConsortium(date),
                Times.Once
            );
        }

        [Fact]
        public async Task Execute_ShouldPropagateException_WhenRepositoryThrows()
        {
            var date = DateTime.Now;

            _repositoryMock
                .Setup(r => r.GetAllInvoicesByMonthAndConsortium(date))
                .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _useCase.Execute(date));

            _repositoryMock.Verify(
                r => r.GetAllInvoicesByMonthAndConsortium(date),
                Times.Once
            );
        }
    }
}
