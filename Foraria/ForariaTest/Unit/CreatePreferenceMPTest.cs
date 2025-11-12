//using Foraria.Domain.Repository;
//using ForariaDomain.Application.UseCase;
//using ForariaDomain.Repository;
//using ForariaDomain.Services;
//using ForariaDomain;
//using Moq;

//namespace ForariaTest.Unit
//{
//    public class CreatePreferenceMPTests
//    {
//        [Fact]
//        public async Task ExecuteAsync_ShouldCreatePreferenceAndSavePayment()
//        {
//            var expenseRepoMock = new Mock<IExpenseDetailRepository>();
//            var paymentRepoMock = new Mock<IPaymentRepository>();
//            var paymentGatewayMock = new Mock<IPaymentGateway>();

//            var expense = new ExpenseDetailByResidence { Id = 1, TotalAmount = 5000 };
//            expenseRepoMock.Setup(r => r.GetExpenseDetailById(1)).ReturnsAsync(expense);

//            paymentGatewayMock.Setup(g => g.CreatePreferenceAsync(5000m, 1, 10))
//                .ReturnsAsync(("pref_123", "https://init-point"));

//            var useCase = new CreatePreferenceMP(
//                paymentRepoMock.Object,
//                paymentGatewayMock.Object,
//                 expenseRepoMock.Object
//            );

//            var result = await useCase.ExecuteAsync(1, 10);

//            Assert.Equal("pref_123", result.PreferenceId);
//            Assert.Equal("https://init-point", result.InitPoint);

//            paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
//            paymentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
//        }
//    }
//}
