using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaTest.Unit.Suppliers;

public class GetSupplierByIdTests
{

    private readonly Mock<ISupplierRepository> _mockSupplierRepository;
    private readonly GetSupplierById _getSupplierById;

    public GetSupplierByIdTests()
    {
        _mockSupplierRepository = new Mock<ISupplierRepository>();
        _getSupplierById = new GetSupplierById(_mockSupplierRepository.Object);
    }

    [Fact]
    public void Execute_ExistingSupplier_ReturnsSupplier()
    {
        // Arrange
        var supplierId = 1;
        var expectedSupplier = new Supplier
        {
            Id = supplierId,
            CommercialName = "Acme Limpieza",
            BusinessName = "Acme Servicios S.A.",
            Cuit = "20-12345678-9",
            SupplierCategory = "Limpieza",
            Email = "contacto@acme.com",
            Phone = "+54 11 1234-5678",
            Address = "Av. Corrientes 1234, CABA",
            ContactPerson = "Juan Pérez",
            Observations = "Proveedor de confianza",
            Active = true,
            RegistrationDate = new DateTime(2024, 1, 15)
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns(expectedSupplier);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSupplier.Id, result.Id);
        Assert.Equal(expectedSupplier.CommercialName, result.CommercialName);
        Assert.Equal(expectedSupplier.BusinessName, result.BusinessName);
        Assert.Equal(expectedSupplier.Cuit, result.Cuit);
        Assert.Equal(expectedSupplier.SupplierCategory, result.SupplierCategory);
        Assert.Equal(expectedSupplier.Email, result.Email);
        Assert.Equal(expectedSupplier.Phone, result.Phone);
        Assert.Equal(expectedSupplier.Address, result.Address);
        Assert.Equal(expectedSupplier.ContactPerson, result.ContactPerson);
        Assert.Equal(expectedSupplier.Observations, result.Observations);
        Assert.True(result.Active);
        Assert.Equal(expectedSupplier.RegistrationDate, result.RegistrationDate);

        _mockSupplierRepository.Verify(r => r.GetById(supplierId), Times.Once);
    }

    [Fact]
    public void Execute_NonExistingSupplier_ReturnsNull()
    {
        // Arrange
        var supplierId = 999;
        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns((Supplier?)null);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.Null(result);
        _mockSupplierRepository.Verify(r => r.GetById(supplierId), Times.Once);
    }

    [Fact]
    public void Execute_SupplierWithAllFields_ReturnsCompleteData()
    {
        // Arrange
        var supplierId = 5;
        var supplier = new Supplier
        {
            Id = supplierId,
            CommercialName = "PlomeroExpress",
            BusinessName = "Plomeros Unidos S.R.L.",
            Cuit = "30-98765432-8",
            SupplierCategory = "Plomería",
            Email = "plomero@express.com",
            Phone = "+54 11 9876-5432",
            Address = "Calle Falsa 123, Buenos Aires",
            ContactPerson = "María García",
            Observations = "Atención 24/7",
            Active = true,
            RegistrationDate = new DateTime(2023, 6, 10)
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns(supplier);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PlomeroExpress", result.CommercialName);
        Assert.Equal("Plomeros Unidos S.R.L.", result.BusinessName);
        Assert.Equal("30-98765432-8", result.Cuit);
        Assert.Equal("Plomería", result.SupplierCategory);
        Assert.Equal("plomero@express.com", result.Email);
        Assert.Equal("+54 11 9876-5432", result.Phone);
        Assert.Equal("Calle Falsa 123, Buenos Aires", result.Address);
        Assert.Equal("María García", result.ContactPerson);
        Assert.Equal("Atención 24/7", result.Observations);
        Assert.True(result.Active);
        Assert.Equal(new DateTime(2023, 6, 10), result.RegistrationDate);
    }

    [Fact]
    public void Execute_InactiveSupplier_ReturnsSupplierWithActiveFalse()
    {
        // Arrange
        var supplierId = 3;
        var inactiveSupplier = new Supplier
        {
            Id = supplierId,
            CommercialName = "Old Supplier",
            BusinessName = "Old Supplier S.A.",
            Cuit = "20-11111111-1",
            SupplierCategory = "Mantenimiento",
            Email = "old@supplier.com",
            Phone = "+54 11 1111-1111",
            Address = "Dirección antigua",
            ContactPerson = "Pedro López",
            Observations = "Ya no trabaja con nosotros",
            Active = false,
            RegistrationDate = new DateTime(2020, 1, 1)
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns(inactiveSupplier);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Active);
        Assert.Equal("Old Supplier", result.CommercialName);
    }

    [Fact]
    public void Execute_SupplierWithNullOptionalFields_ReturnsSupplierSuccessfully()
    {
        // Arrange
        var supplierId = 7;
        var supplier = new Supplier
        {
            Id = supplierId,
            CommercialName = "Minimal Supplier",
            BusinessName = "Minimal S.A.",
            Cuit = "20-22222222-2",
            SupplierCategory = "Electricidad",
            Email = "minimal@test.com",
            Phone = null,
            Address = null,
            ContactPerson = null,
            Observations = null,
            Active = true,
            RegistrationDate = DateTime.Now
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns(supplier);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Minimal Supplier", result.CommercialName);
        Assert.Null(result.Phone);
        Assert.Null(result.Address);
        Assert.Null(result.ContactPerson);
        Assert.Null(result.Observations);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void Execute_DifferentSupplierIds_CallsRepositoryWithCorrectId(int supplierId)
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = supplierId,
            CommercialName = "Test",
            BusinessName = "Test S.A.",
            Cuit = "20-12345678-9",
            SupplierCategory = "Test",
            Email = "test@test.com",
            Active = true,
            RegistrationDate = DateTime.Now
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(supplierId))
            .Returns(supplier);

        // Act
        var result = _getSupplierById.Execute(supplierId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplierId, result.Id);
        _mockSupplierRepository.Verify(r => r.GetById(supplierId), Times.Once);
    }

}
