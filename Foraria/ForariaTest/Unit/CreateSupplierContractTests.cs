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

public class CreateSupplierContractTests
{

    private readonly Mock<ISupplierContractRepository> _mockContractRepository;
    private readonly Mock<ISupplierRepository> _mockSupplierRepository;
    private readonly CreateSupplierContract _createSupplierContract;

    public CreateSupplierContractTests()
    {
        _mockContractRepository = new Mock<ISupplierContractRepository>();
        _mockSupplierRepository = new Mock<ISupplierRepository>();
        _createSupplierContract = new CreateSupplierContract(
            _mockContractRepository.Object,
            _mockSupplierRepository.Object
        );
    }

    [Fact]
    public void Execute_ValidContract_CreatesContractSuccessfully()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            BusinessName = "Proveedor Test S.A.",
            Cuit = "20-12345678-9",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato de Mantenimiento",
            ContractType = "Servicios",
            Description = "Mantenimiento mensual",
            MonthlyAmount = 15000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        _mockContractRepository
            .Setup(r => r.GetBySupplierId(1))
            .Returns(new List<SupplierContract>());

        _mockContractRepository
            .Setup(r => r.Create(It.IsAny<SupplierContract>()))
            .Returns((SupplierContract c) => { c.Id = 1; return c; });

        // Act
        var result = _createSupplierContract.Execute(contract);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Contrato de Mantenimiento", result.Name);
        Assert.True(result.Active);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
        _mockSupplierRepository.Verify(r => r.GetById(1), Times.Once);
        _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Once);
    }

    [Fact]
    public void Execute_SupplierDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 999
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(999))
            .Returns((Supplier?)null);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("El proveedor con ID 999 no existe", exception.Message);
        _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
    }

    [Fact]
    public void Execute_InactiveSupplier_ThrowsInvalidOperationException()
    {
        // Arrange
        var inactiveSupplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Inactivo",
            Active = false
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(inactiveSupplier);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("está inactivo y no puede tener contratos nuevos", exception.Message);
        _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
    }

    [Fact]
    public void Execute_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1), // Fecha de fin anterior a la de inicio
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("La fecha de vencimiento debe ser posterior a la fecha de inicio", exception.Message);
    }

    [Fact]
    public void Execute_StartDateTooOld_ThrowsArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow.AddYears(-11), // Más de 10 años atrás
            EndDate = DateTime.UtcNow.AddYears(-10),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("La fecha de inicio no puede ser mayor a 10 años en el pasado", exception.Message);
    }

    [Fact]
    public void Execute_NegativeMonthlyAmount_ThrowsArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = -100, // Monto negativo
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("El monto mensual debe ser mayor a 0", exception.Message);
    }

    [Fact]
    public void Execute_ZeroMonthlyAmount_ThrowsArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 0, // Monto cero
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _createSupplierContract.Execute(contract));
        Assert.Contains("El monto mensual debe ser mayor a 0", exception.Message);
    }

    [Fact]
    public void Execute_DuplicateActiveContractName_ThrowsInvalidOperationException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var existingContract = new SupplierContract
        {
            Id = 10,
            Name = "Contrato Duplicado",
            Active = true,
            SupplierId = 1
        };

        var newContract = new SupplierContract
        {
            Name = "Contrato Duplicado", // Mismo nombre
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        _mockContractRepository
            .Setup(r => r.GetBySupplierId(1))
            .Returns(new List<SupplierContract> { existingContract });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _createSupplierContract.Execute(newContract));
        Assert.Contains("Ya existe un contrato activo con el nombre", exception.Message);
        _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
    }

    [Fact]
    public void Execute_DuplicateNameButInactive_CreatesContractSuccessfully()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var existingInactiveContract = new SupplierContract
        {
            Id = 10,
            Name = "Contrato Duplicado",
            Active = false, // Inactivo
            SupplierId = 1
        };

        var newContract = new SupplierContract
        {
            Name = "Contrato Duplicado", // Mismo nombre pero el anterior está inactivo
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        _mockContractRepository
            .Setup(r => r.GetBySupplierId(1))
            .Returns(new List<SupplierContract> { existingInactiveContract });

        _mockContractRepository
            .Setup(r => r.Create(It.IsAny<SupplierContract>()))
            .Returns((SupplierContract c) => { c.Id = 1; return c; });

        // Act
        var result = _createSupplierContract.Execute(newContract);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Active);
        _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Once);
    }

    [Fact]
    public void Execute_CaseInsensitiveDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var existingContract = new SupplierContract
        {
            Id = 10,
            Name = "contrato test", // Minúsculas
            Active = true,
            SupplierId = 1
        };

        var newContract = new SupplierContract
        {
            Name = "CONTRATO TEST", // Mayúsculas
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        _mockContractRepository
            .Setup(r => r.GetBySupplierId(1))
            .Returns(new List<SupplierContract> { existingContract });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _createSupplierContract.Execute(newContract));
        Assert.Contains("Ya existe un contrato activo con el nombre", exception.Message);
    }

    [Fact]
    public void Execute_SetsActiveAndCreatedAtAutomatically()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = 1,
            CommercialName = "Proveedor Test",
            Active = true
        };

        var contract = new SupplierContract
        {
            Name = "Contrato Test",
            MonthlyAmount = 10000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            SupplierId = 1,
            Active = false, 
            CreatedAt = DateTime.MinValue 
        };

        _mockSupplierRepository
            .Setup(r => r.GetById(1))
            .Returns(supplier);

        _mockContractRepository
            .Setup(r => r.GetBySupplierId(1))
            .Returns(new List<SupplierContract>());

        _mockContractRepository
            .Setup(r => r.Create(It.IsAny<SupplierContract>()))
            .Returns((SupplierContract c) => c);

        // Act
        var result = _createSupplierContract.Execute(contract);

        // Assert
        Assert.True(result.Active); // Se establece en true automáticamente
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt); // Se establece la fecha actual
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.CreatedAt >= DateTime.UtcNow.AddSeconds(-5)); // Creado hace menos de 5 segundos
    }

}
