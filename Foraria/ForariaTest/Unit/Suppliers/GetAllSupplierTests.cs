using System.Collections.Generic;
using Xunit;
using Moq;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Suppliers
{
    public class GetAllSupplierTests
    {
        [Fact]
        public async Task Execute_ShouldReturnListOfSuppliers()
        {
            var mockRepository = new Mock<ISupplierRepository>();

            var suppliersMock = new List<Supplier>
            {
                new Supplier { Id = 1, CommercialName = "Proveedor 1" },
                new Supplier { Id = 2, CommercialName = "Proveedor 2" }
            };

            int consortiumId = 10;

            mockRepository
                .Setup(repo => repo.GetAll(consortiumId))
                .ReturnsAsync(suppliersMock);

            var useCase = new GetAllSupplier(mockRepository.Object);

            var result = await useCase.Execute(consortiumId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Proveedor 1", result[0].CommercialName);

            mockRepository.Verify(repo => repo.GetAll(consortiumId), Times.Once);
        }
    }
}
