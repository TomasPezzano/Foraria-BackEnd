using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForariaTest.Unit.Suppliers;

public class DeleteSupplierTest
{
    [Fact]
    public async Task Execute_WhenSupplierExistsAndHasNoActiveContracts_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Cuit = "20-12345678-9",
            Contracts = new List<SupplierContract>() 
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .ReturnsAsync(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.GetById(1), Times.Once);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenSupplierExistsWithInactiveContracts_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Cuit = "20-12345678-9",
            Contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = false },
                new SupplierContract { Id = 2, Active = false }
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .ReturnsAsync(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenSupplierDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        mockRepository
            .Setup(r => r.GetById(999))
            .ReturnsAsync((Supplier?)null);

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => useCase.ExecuteAsync(999));
        Assert.Equal("El proveedor con ID 999 no existe", exception.Message);

        mockRepository.Verify(r => r.GetById(999), Times.Once);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenSupplierHasActiveContracts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Cuit = "20-12345678-9",
            Contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = true, Name = "Mantenimiento mensual", MonthlyAmount = 50000 }
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .ReturnsAsync(supplier);

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(1));

        mockRepository.Verify(r => r.GetById(1), Times.Once);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenSupplierHasMixedContracts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = false },
                new SupplierContract { Id = 2, Active = true },
                new SupplierContract { Id = 3, Active = false }
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .ReturnsAsync(supplier);

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(1));

        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenSupplierContractsIsNull_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Contracts = null
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .ReturnsAsync(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }
}
