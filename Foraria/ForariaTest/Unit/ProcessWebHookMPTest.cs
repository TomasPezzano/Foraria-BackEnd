using System.Text.Json;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using ForariaDomain;
using Moq;
using Foraria.Contracts.DTOs;

namespace ForariaTest.Unit
{
    public class ProcessWebHookMPTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldApprovePaymentAndMarkExpenseAsPaid()
        {
            // Arrange
            var paymentRepo = new Mock<IPaymentRepository>();
            var expenseRepo = new Mock<IExpenseRepository>();
            var paymentMethodRepo = new Mock<IPaymentMethodRepository>();
            var gateway = new Mock<IPaymentGateway>();

            var bodyJson = """
        {
            "data": {
                "id": "9999"
            }
        }
        """;

            var body = JsonDocument.Parse(bodyJson).RootElement;

            var mpPayment = new MercadoPagoPaymentDto
            {
                Id = 9999,
                Status = "approved",
                StatusDetail = "accredited",
                TransactionAmount = 5000m,
                Metadata = new Dictionary<string, object>
            {
                { "expense_id", 1 },
                { "residence_id", 10 }
            },
                Order = new MercadoPagoOrderDto { Id = 123 }
            };

            gateway.Setup(g => g.GetPaymentAsync(9999)).ReturnsAsync(mpPayment);
            gateway.Setup(g => g.VerifyMerchantOrderAsync(123)).ReturnsAsync(true);

            var existingPayment = new Payment { Id = 1, ExpenseId = 1, Status = "pending" };
            var expense = new Expense { Id = 1, State = "unpaid" };

            paymentRepo.Setup(r => r.FindByMercadoPagoMetadataAsync(
                It.IsAny<Dictionary<string, object>>(), "123", "9999"))
                .ReturnsAsync(existingPayment);

            expenseRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);

            var useCase = new ProcessWebHookMP(
                paymentRepo.Object, expenseRepo.Object, paymentMethodRepo.Object, gateway.Object
            );

            await useCase.ExecuteAsync(body);

            Assert.Equal("approved", existingPayment.Status);
            Assert.Equal("paid", expense.State);

            paymentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            expenseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
