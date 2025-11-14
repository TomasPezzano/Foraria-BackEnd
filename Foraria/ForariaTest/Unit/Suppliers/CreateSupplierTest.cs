using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;

namespace ForariaTest.Unit.Suppliers
{
    public class CreateSupplierTest
    {
        [Fact]
        public async Task Execute_WhenSupplierIsValid_ShouldCreateSuccessfully()
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Plomería San Martin",
                BusinessName = "San Martin SRL",
                Cuit = "20-12345678-9",
                SupplierCategory = "Plomería",
                Email = "contacto@sanmartin.com",
                Phone = "1155667788"
            };

            var expectedSupplier = new Supplier
            {
                Id = 1,
                CommercialName = supplier.CommercialName,
                BusinessName = supplier.BusinessName,
                Cuit = supplier.Cuit,
                SupplierCategory = supplier.SupplierCategory,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Active = true
            };

            mockRepository
                .Setup(r => r.Create(It.IsAny<Supplier>()))
                .ReturnsAsync(expectedSupplier);

            var useCase = new CreateSupplier(mockRepository.Object);

            var result = await useCase.Execute(supplier);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Plomería San Martin", result.CommercialName);
            Assert.True(result.Active);
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenCuitHasInvalidLength_ShouldThrowArgumentException()
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Plomería San Martin",
                Cuit = "20-123456-9", 
                SupplierCategory = "Plomería"
            };

            var useCase = new CreateSupplier(mockRepository.Object);

            
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(supplier));
            Assert.Equal("El CUIT debe tener 11 dígitos", ex.Message);
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenCuitHasLetters_ShouldThrowArgumentException()
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Plomería San Martin",
                Cuit = "20-1234567A-9", 
                SupplierCategory = "Plomería"
            };

            var useCase = new CreateSupplier(mockRepository.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(supplier));
            Assert.Equal("El CUIT debe tener 11 dígitos", ex.Message);
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenCuitHasDashesButIsValid_ShouldCreateSuccessfully()
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Plomería San Martin",
                Cuit = "20-12345678-9", 
                SupplierCategory = "Plomería"
            };

            var expectedSupplier = new Supplier { Id = 1, Cuit = supplier.Cuit };

            mockRepository
                .Setup(r => r.Create(It.IsAny<Supplier>()))
                .ReturnsAsync(expectedSupplier);

            var useCase = new CreateSupplier(mockRepository.Object);

          
            var result = await useCase.Execute(supplier);

            Assert.NotNull(result);
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
        }

        [Theory]
        [InlineData("12345678901")] 
        [InlineData("20-12345678-9")]
        [InlineData("20123456789")] 
        public async Task Execute_WhenCuitIsValid_ShouldAcceptDifferentFormats(string cuit)
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Test Supplier",
                Cuit = cuit,
                SupplierCategory = "Test"
            };

            mockRepository
                .Setup(r => r.Create(It.IsAny<Supplier>()))
                .ReturnsAsync(new Supplier { Id = 1 });

            var useCase = new CreateSupplier(mockRepository.Object);

            var result = await useCase.Execute(supplier);

            Assert.NotNull(result);
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
        }

        [Theory]
        [InlineData("")] 
        [InlineData("123")] 
        [InlineData("20-12345678-9-1")] 
        [InlineData("ABCDEFGHIJK")] 
        public async Task Execute_WhenCuitIsInvalid_ShouldThrowArgumentException(string invalidCuit)
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var supplier = new Supplier
            {
                CommercialName = "Test Supplier",
                Cuit = invalidCuit,
                SupplierCategory = "Test"
            };

            var useCase = new CreateSupplier(mockRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => useCase.Execute(supplier));
            mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
        }
    }
}
