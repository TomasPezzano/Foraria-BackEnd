using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForariaTest.Unit.Suppliers
{
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
        public async Task Execute_ValidContract_CreatesContractSuccessfully()
        {
            var supplier = new Supplier
            {
                Id = 1,
                CommercialName = "Proveedor Test",
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

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            _mockContractRepository.Setup(r => r.GetBySupplierId(1))
                .ReturnsAsync(new List<SupplierContract>());

            _mockContractRepository.Setup(r => r.Create(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) =>
                {
                    c.Id = 1;
                    return c;
                });

            var result = await _createSupplierContract.Execute(contract);

            Assert.NotNull(result);
            Assert.Equal("Contrato de Mantenimiento", result.Name);
            Assert.True(result.Active);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);

            _mockSupplierRepository.Verify(r => r.GetById(1), Times.Once);
            _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Once);
        }


        [Fact]
        public async Task Execute_SupplierDoesNotExist_ThrowsArgumentException()
        {
            var contract = new SupplierContract
            {
                Name = "Contrato Test",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 999
            };

            _mockSupplierRepository.Setup(r => r.GetById(999))
                .ReturnsAsync((Supplier?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("El proveedor con ID 999 no existe.", exception.Message);

            _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
        }


        [Fact]
        public async Task Execute_InactiveSupplier_ThrowsInvalidOperationException()
        {
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

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(inactiveSupplier);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("está inactivo y no puede tener contratos nuevos", exception.Message);

            _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
        }


        [Fact]
        public async Task Execute_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var contract = new SupplierContract
            {
                Name = "Contrato Test",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("La fecha de vencimiento debe ser posterior a la fecha de inicio.", exception.Message);
        }


        [Fact]
        public async Task Execute_StartDateTooOld_ThrowsArgumentException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var contract = new SupplierContract
            {
                Name = "Contrato Test",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow.AddYears(-11),
                EndDate = DateTime.UtcNow.AddYears(-10),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("La fecha de inicio no puede ser mayor a 10 años en el pasado.", exception.Message);
        }


        [Fact]
        public async Task Execute_NegativeMonthlyAmount_ThrowsArgumentException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var contract = new SupplierContract
            {
                Name = "Contrato Test",
                MonthlyAmount = -100,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("El monto mensual debe ser mayor a 0.", exception.Message);
        }


        [Fact]
        public async Task Execute_ZeroMonthlyAmount_ThrowsArgumentException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var contract = new SupplierContract
            {
                Name = "Contrato Test",
                MonthlyAmount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _createSupplierContract.Execute(contract));

            Assert.Contains("El monto mensual debe ser mayor a 0.", exception.Message);
        }


        [Fact]
        public async Task Execute_DuplicateActiveContractName_ThrowsInvalidOperationException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var existing = new SupplierContract
            {
                Id = 10,
                Name = "Contrato Duplicado",
                Active = true,
                SupplierId = 1
            };

            var newContract = new SupplierContract
            {
                Name = "Contrato Duplicado",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            _mockContractRepository.Setup(r => r.GetBySupplierId(1))
                .ReturnsAsync(new List<SupplierContract> { existing });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _createSupplierContract.Execute(newContract));

            Assert.Contains("Ya existe un contrato activo con el nombre", exception.Message);

            _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Never);
        }


        [Fact]
        public async Task Execute_DuplicateNameButInactive_CreatesContractSuccessfully()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var existingInactive = new SupplierContract
            {
                Id = 10,
                Name = "Contrato Duplicado",
                Active = false,
                SupplierId = 1
            };

            var newContract = new SupplierContract
            {
                Name = "Contrato Duplicado",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            _mockContractRepository.Setup(r => r.GetBySupplierId(1))
                .ReturnsAsync(new List<SupplierContract> { existingInactive });

            _mockContractRepository.Setup(r => r.Create(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            var result = await _createSupplierContract.Execute(newContract);

            Assert.NotNull(result);
            Assert.True(result.Active);

            _mockContractRepository.Verify(r => r.Create(It.IsAny<SupplierContract>()), Times.Once);
        }


        [Fact]
        public async Task Execute_CaseInsensitiveDuplicateName_ThrowsInvalidOperationException()
        {
            var supplier = new Supplier { Id = 1, CommercialName = "Test", Active = true };

            var existing = new SupplierContract
            {
                Id = 10,
                Name = "contrato test",
                Active = true,
                SupplierId = 1
            };

            var newContract = new SupplierContract
            {
                Name = "CONTRATO TEST",
                MonthlyAmount = 10000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                SupplierId = 1
            };

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            _mockContractRepository.Setup(r => r.GetBySupplierId(1))
                .ReturnsAsync(new List<SupplierContract> { existing });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _createSupplierContract.Execute(newContract));

            Assert.Contains("Ya existe un contrato activo con el nombre", exception.Message);
        }


        [Fact]
        public async Task Execute_SetsActiveAndCreatedAtAutomatically()
        {
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

            _mockSupplierRepository.Setup(r => r.GetById(1))
                .ReturnsAsync(supplier);

            _mockContractRepository.Setup(r => r.GetBySupplierId(1))
                .ReturnsAsync(new List<SupplierContract>());

            _mockContractRepository.Setup(r => r.Create(It.IsAny<SupplierContract>()))
                .ReturnsAsync((SupplierContract c) => c);

            var result = await _createSupplierContract.Execute(contract);

            Assert.True(result.Active);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);

            // margen razonable
            Assert.True(result.CreatedAt <= DateTime.UtcNow);
            Assert.True(result.CreatedAt >= DateTime.UtcNow.AddSeconds(-10));
        }
    }
}
