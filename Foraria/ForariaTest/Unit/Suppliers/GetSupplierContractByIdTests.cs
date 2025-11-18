using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace ForariaTest.Unit.Suppliers
{
    public class GetSupplierContractByIdTests
    {
        private readonly Mock<ISupplierContractRepository> _mockContractRepo;
        private readonly Mock<IContractExpirationService> _mockExpirationService;
        private readonly GetSupplierContractById _useCase;

        public GetSupplierContractByIdTests()
        {
            _mockContractRepo = new Mock<ISupplierContractRepository>();
            _mockExpirationService = new Mock<IContractExpirationService>();
            _useCase = new GetSupplierContractById(_mockContractRepo.Object, _mockExpirationService.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenContractNotFound()
        {
            // Arrange
            _mockContractRepo.Setup(r => r.GetById(It.IsAny<int>()))
                             .ReturnsAsync((SupplierContract?)null);

            // Act
            var result = await _useCase.Execute(99);

            // Assert
            Assert.Null(result);

            _mockExpirationService.Verify(s => s.CheckAndUpdateExpiration(It.IsAny<SupplierContract>()), Times.Never);
            _mockContractRepo.Verify(r => r.Update(It.IsAny<SupplierContract>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldReturnActiveContract_WhenStillValid()
        {
            // Arrange
            var contract = new SupplierContract { Id = 1, Active = true };

            _mockContractRepo.Setup(r => r.GetById(1))
                             .ReturnsAsync(contract);

            // No cambia el estado
            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(contract))
                                  .Callback<SupplierContract>(c => { /* sigue activo */ });

            // Act
            var result = await _useCase.Execute(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Active);

            _mockContractRepo.Verify(r => r.Update(It.IsAny<SupplierContract>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldUpdateContract_WhenItBecomesInactive()
        {
            // Arrange
            var contract = new SupplierContract { Id = 2, Active = true };

            _mockContractRepo.Setup(r => r.GetById(2))
                             .ReturnsAsync(contract);

            // El servicio lo marca como vencido
            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(contract))
                                  .Callback<SupplierContract>(c => c.Active = false);

            // Act
            var result = await _useCase.Execute(2);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Active);

            _mockContractRepo.Verify(
                r => r.Update(It.Is<SupplierContract>(c => c.Id == 2 && c.Active == false)),
                Times.Once
            );
        }
    }
}
