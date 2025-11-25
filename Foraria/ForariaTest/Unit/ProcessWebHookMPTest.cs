using System.Text.Json;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain;
using Moq;
using ForariaDomain.Services;
using ForariaDomain.Models;


namespace ForariaTest.Unit
{
    public class ProcessWebHookMPTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldApprovePaymentAndMarkExpenseAsPaid()
        {
            var paymentRepo = new Mock<IPaymentRepository>();
            var expenseRepo = new Mock<IExpenseDetailRepository>();
            var paymentMethodRepo = new Mock<IPaymentMethodRepository>();
            var gateway = new Mock<IPaymentService>();

            var bodyJson = """
        {
            "data": {
                "id": "9999"
            }
        }
        """;

            var body = JsonDocument.Parse(bodyJson).RootElement;

            var mpPayment = new MercadoPagoPayment
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
                Order = new MercadoPagoOrder { Id = 123 }
            };

            gateway.Setup(g => g.GetPaymentAsync(9999)).ReturnsAsync(mpPayment);
            gateway.Setup(g => g.VerifyMerchantOrderAsync(123)).ReturnsAsync(true);

            var existingPayment = new Payment { Id = 1, ExpenseDetailByResidenceId = 1, Status = "pending" };
            var expense = new ExpenseDetailByResidence { Id = 1, State = "unpaid" };

            paymentRepo.Setup(r => r.FindByMercadoPagoMetadataAsync(
                It.IsAny<Dictionary<string, object>>(), "123", "9999"))
                .ReturnsAsync(existingPayment);

            expenseRepo.Setup(r => r.GetExpenseDetailById(1)).ReturnsAsync(expense);

            var useCase = new ProcessWebHookMP(
                paymentRepo.Object, paymentMethodRepo.Object, gateway.Object, expenseRepo.Object
            );

            await useCase.ExecuteAsync(body);

            Assert.Equal("approved", existingPayment.Status);
            Assert.Equal("paid", expense.State);

            paymentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            expenseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
