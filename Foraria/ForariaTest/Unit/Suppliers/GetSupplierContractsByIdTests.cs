using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ForariaTest.Unit.Suppliers
{
    public class GetSupplierContractsByIdTests
    {
        private readonly Mock<ISupplierContractRepository> _mockContractRepo;
        private readonly Mock<IContractExpirationService> _mockExpirationService;
        private readonly GetSupplierContractsById _useCase;

        public GetSupplierContractsByIdTests()
        {
            _mockContractRepo = new Mock<ISupplierContractRepository>();
            _mockExpirationService = new Mock<IContractExpirationService>();
            _useCase = new GetSupplierContractsById(_mockContractRepo.Object, _mockExpirationService.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnEmptyList_WhenNoContractsExist()
        {
            // Arrange
            _mockContractRepo
                .Setup(r => r.GetBySupplierId(It.IsAny<int>()))
                .ReturnsAsync(new List<SupplierContract>());

            // Ensure Update returns a completed Task if ever called
            _mockContractRepo
                .Setup(r => r.Update(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            // Act
            var result = await _useCase.Execute(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockExpirationService.Verify(s => s.CheckAndUpdateExpiration(It.IsAny<SupplierContract>()), Times.Never);
            _mockContractRepo.Verify(r => r.Update(It.IsAny<SupplierContract>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldNotUpdate_WhenContractsRemainActive()
        {
            // Arrange
            var contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = true },
                new SupplierContract { Id = 2, Active = true }
            };

            _mockContractRepo.Setup(r => r.GetBySupplierId(1)).ReturnsAsync(contracts);

            // If Update is awaited, return the same contract to avoid null-await
            _mockContractRepo
                .Setup(r => r.Update(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            // El servicio no cambia nada → siguen activos
            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(It.IsAny<SupplierContract>()))
                                  .Callback<SupplierContract>(c => { /* no cambia */ });

            // Act
            var result = await _useCase.Execute(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.True(c.Active));

            _mockContractRepo.Verify(r => r.Update(It.IsAny<SupplierContract>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldUpdate_WhenContractBecomesInactive()
        {
            // Arrange
            var contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = true },
                new SupplierContract { Id = 2, Active = true }
            };

            _mockContractRepo.Setup(r => r.GetBySupplierId(1)).ReturnsAsync(contracts);

            // Make Update return the contract to avoid null-await
            _mockContractRepo
                .Setup(r => r.Update(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            // Solo el contrato 1 cambia estado a inactivo
            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(It.Is<SupplierContract>(c => c.Id == 1)))
                                  .Callback<SupplierContract>(c => c.Active = false);

            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(It.Is<SupplierContract>(c => c.Id == 2)))
                                  .Callback<SupplierContract>(c => { c.Active = true; });

            // Act  
            var result = await _useCase.Execute(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.False(result.First(c => c.Id == 1).Active);

            _mockContractRepo.Verify(r => r.Update(It.Is<SupplierContract>(c => c.Id == 1)), Times.Once);
            _mockContractRepo.Verify(r => r.Update(It.Is<SupplierContract>(c => c.Id == 2)), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldUpdateAll_WhenAllContractsExpire()
        {
            // Arrange
            var contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = true },
                new SupplierContract { Id = 2, Active = true }
            };

            _mockContractRepo.Setup(r => r.GetBySupplierId(It.IsAny<int>())).ReturnsAsync(contracts);

            // Todos pasan a inactivos
            _mockExpirationService.Setup(s => s.CheckAndUpdateExpiration(It.IsAny<SupplierContract>()))
                                  .Callback<SupplierContract>(c => c.Active = false);

            // Ensure Update returns a completed Task when awaited
            _mockContractRepo
                .Setup(r => r.Update(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            // Act
            var result = await _useCase.Execute(99);

            // Assert
            Assert.All(result, c => Assert.False(c.Active));

            // Se actualizan los dos
            _mockContractRepo.Verify(r => r.Update(It.IsAny<SupplierContract>()), Times.Exactly(2));
        }
    }
}
