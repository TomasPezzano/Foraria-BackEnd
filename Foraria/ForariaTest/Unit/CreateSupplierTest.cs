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

public class CreateSupplierTest
{
    [Fact]
    public void Execute_WhenSupplierIsValid_ShouldCreateSuccessfully()
    {
        // Arrange
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
            .Returns(expectedSupplier);

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(supplier);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Plomería San Martin", result.CommercialName);
        Assert.True(result.Active);
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
    }

    [Fact]
    public void Execute_WhenCuitHasInvalidLength_ShouldThrowArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            CommercialName = "Plomería San Martin",
            Cuit = "20-123456-9", // Solo 9 dígitos, debería tener 11
            SupplierCategory = "Plomería"
        };

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => useCase.Execute(supplier));
        Assert.Equal("El CUIT debe tener 11 dígitos", exception.Message);
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
    }

    [Fact]
    public void Execute_WhenCuitHasLetters_ShouldThrowArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            CommercialName = "Plomería San Martin",
            Cuit = "20-1234567A-9", // Tiene una letra
            SupplierCategory = "Plomería"
        };

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => useCase.Execute(supplier));
        Assert.Equal("El CUIT debe tener 11 dígitos", exception.Message);
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
    }

    [Fact]
    public void Execute_WhenCuitHasDashesButIsValid_ShouldCreateSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            CommercialName = "Plomería San Martin",
            Cuit = "20-12345678-9", // Con guiones pero 11 dígitos
            SupplierCategory = "Plomería"
        };

        var expectedSupplier = new Supplier { Id = 1, Cuit = supplier.Cuit };

        mockRepository
            .Setup(r => r.Create(It.IsAny<Supplier>()))
            .Returns(expectedSupplier);

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(supplier);

        // Assert
        Assert.NotNull(result);
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
    }

    [Theory]
    [InlineData("12345678901")] // Sin guiones
    [InlineData("20-12345678-9")] // Con guiones
    [InlineData("20123456789")] // Sin guiones pero 11 dígitos
    public void Execute_WhenCuitIsValid_ShouldAcceptDifferentFormats(string cuit)
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            CommercialName = "Test Supplier",
            Cuit = cuit,
            SupplierCategory = "Test"
        };

        mockRepository
            .Setup(r => r.Create(It.IsAny<Supplier>()))
            .Returns(new Supplier { Id = 1 });

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act
        var result = useCase.Execute(supplier);

        // Assert
        Assert.NotNull(result);
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Once);
    }

    [Theory]
    [InlineData("")] // Vacío
    [InlineData("123")] // Muy corto
    [InlineData("20-12345678-9-1")] // Muy largo
    [InlineData("ABCDEFGHIJK")] // Solo letras
    public void Execute_WhenCuitIsInvalid_ShouldThrowArgumentException(string invalidCuit)
    {
        // Arrange
        var mockRepository = new Mock<ISupplierRepository>();

        var supplier = new Supplier
        {
            CommercialName = "Test Supplier",
            Cuit = invalidCuit,
            SupplierCategory = "Test"
        };

        var useCase = new CreateSupplier(mockRepository.Object);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => useCase.Execute(supplier));
        mockRepository.Verify(r => r.Create(It.IsAny<Supplier>()), Times.Never);
    }
}
