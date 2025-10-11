using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit;

public class DeleteSupplierTest
{
    [Fact]
    public void Execute_WhenSupplierExistsAndHasNoActiveContracts_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Cuit = "20-12345678-9",
            Contracts = new List<SupplierContract>() // Sin contratos
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.GetById(1), Times.Once);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public void Execute_WhenSupplierExistsWithInactiveContracts_ShouldDeleteSuccessfully()
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
                new SupplierContract { Id = 1, Active = false }, // Contrato inactivo
                new SupplierContract { Id = 2, Active = false }  // Contrato inactivo
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public void Execute_WhenSupplierDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        mockRepository
            .Setup(r => r.GetById(999))
            .Returns((Supplier?)null); // No existe

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => useCase.Execute(999));
        Assert.Equal("El proveedor con ID 999 no existe", exception.Message);
        mockRepository.Verify(r => r.GetById(999), Times.Once);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Execute_WhenSupplierHasActiveContracts_ShouldThrowInvalidOperationException()
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
                new SupplierContract
                {
                    Id = 1,
                    Active = true, // ← Contrato activo
                    Name = "Mantenimiento mensual",
                    MonthlyAmount = 50000
                }
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute(1));
        Assert.Equal("No se puede eliminar un proveedor con contratos activos", exception.Message);
        mockRepository.Verify(r => r.GetById(1), Times.Once);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Execute_WhenSupplierHasMixedContracts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Contracts = new List<SupplierContract>
            {
                new SupplierContract { Id = 1, Active = false }, // Inactivo
                new SupplierContract { Id = 2, Active = true },  // ← ACTIVO (bloquea eliminación)
                new SupplierContract { Id = 3, Active = false }  // Inactivo
            }
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => useCase.Execute(1));
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Execute_WhenSupplierContractsIsNull_ShouldDeleteSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Plomería San Martin",
            Contracts = null // ← Null (no se cargaron contratos)
        };

        mockRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        mockRepository
            .Setup(r => r.Delete(1))
            .Verifiable();

        var useCase = new DeleteSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(1);

        // Assert
        Assert.True(result);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }
}
