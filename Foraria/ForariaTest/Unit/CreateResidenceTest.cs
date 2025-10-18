using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ForariaTest.Application
{
    public class CreateResidenceUseCaseTests
    {
        private readonly Mock<IResidenceRepository> _mockResidenceRepository;
        private readonly CreateResidence _createResidenceUseCase;

        public CreateResidenceUseCaseTests()
        {
            _mockResidenceRepository = new Mock<IResidenceRepository>();
            _createResidenceUseCase = new CreateResidence(_mockResidenceRepository.Object);
        }

        #region Create Tests

        [Fact]
        public async Task Create_ValidResidence_ReturnsSuccessResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 1,
                Tower = "A"
            };

            var createdResidence = new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll())
                .ReturnsAsync(new List<Residence>());

            _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>()))
                .ReturnsAsync(createdResidence);

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Vivienda creada exitosamente", result.Message);
            Assert.Equal(101, result.Number);
            Assert.Equal(1, result.Floor);
            Assert.Equal("A", result.Tower);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Create_InvalidTower_ReturnsFailureResponse(string invalidTower)
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 1,
                Tower = invalidTower
            };

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("La torre no puede estar vacía", result.Message);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Never);
        }

        [Fact]
        public async Task Create_DuplicateResidence_ReturnsFailureResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 1,
                Tower = "A"
            };

            var existing = new List<Residence>
            {
                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll()).ReturnsAsync(existing);

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Ya existe una vivienda con ese número, piso y torre", result.Message);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Never);
        }

        [Fact]
        public async Task Create_DuplicateResidenceDifferentCase_ReturnsFailureResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 1,
                Tower = "a"
            };

            var existing = new List<Residence>
            {
                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll()).ReturnsAsync(existing);

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Ya existe una vivienda con ese número, piso y torre", result.Message);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Never);
        }

        [Fact]
        public async Task Create_SameNumberDifferentFloor_ReturnsSuccessResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 2,
                Tower = "A"
            };

            var existing = new List<Residence>
            {
                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
            };

            var created = new Residence
            {
                Id = 2,
                Number = 101,
                Floor = 2,
                Tower = "A"
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll()).ReturnsAsync(existing);
            _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>())).ReturnsAsync(created);

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Vivienda creada exitosamente", result.Message);
            Assert.Equal(2, result.Floor);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
        }

        [Fact]
        public async Task Create_SameNumberDifferentTower_ReturnsSuccessResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Number = 101,
                Floor = 1,
                Tower = "B"
            };

            var existing = new List<Residence>
            {
                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" }
            };

            var created = new Residence
            {
                Id = 2,
                Number = 101,
                Floor = 1,
                Tower = "B"
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll()).ReturnsAsync(existing);
            _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>())).ReturnsAsync(created);

            // Act
            var result = await _createResidenceUseCase.Create(residence);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Vivienda creada exitosamente", result.Message);
            Assert.Equal("B", result.Tower);
            _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
        }

        #endregion

        #region GetResidenceById Tests

        [Fact]
        public async Task GetResidenceById_ExistingResidence_ReturnsSuccessResponse()
        {
            // Arrange
            var residence = new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            };

            _mockResidenceRepository.Setup(repo => repo.GetById(1)).ReturnsAsync(residence);

            // Act
            var result = await _createResidenceUseCase.GetResidenceById(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Vivienda obtenida exitosamente", result.Message);
            Assert.Equal(101, result.Number);
            _mockResidenceRepository.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public async Task GetResidenceById_NonExistingResidence_ReturnsFailureResponse()
        {
            // Arrange
            _mockResidenceRepository.Setup(repo => repo.GetById(999))
                .ReturnsAsync((Residence?)null);

            // Act
            var result = await _createResidenceUseCase.GetResidenceById(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Vivienda no encontrada", result.Message);
            _mockResidenceRepository.Verify(repo => repo.GetById(999), Times.Once);
        }

        #endregion

        #region GetAllResidences Tests

        [Fact]
        public async Task GetAllResidences_WithResidences_ReturnsList()
        {
            // Arrange
            var residences = new List<Residence>
            {
                new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" },
                new Residence { Id = 2, Number = 102, Floor = 1, Tower = "A" }
            };

            _mockResidenceRepository.Setup(repo => repo.GetAll()).ReturnsAsync(residences);

            // Act
            var result = await _createResidenceUseCase.GetAllResidences();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.Success));
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
            _mockResidenceRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllResidences_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            _mockResidenceRepository.Setup(repo => repo.GetAll())
                .ReturnsAsync(new List<Residence>());

            // Act
            var result = await _createResidenceUseCase.GetAllResidences();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockResidenceRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion
    }
}