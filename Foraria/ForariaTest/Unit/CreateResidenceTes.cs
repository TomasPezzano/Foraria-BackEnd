using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Moq;

namespace ForariaTest.Application;

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
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = "A"
        };

        var createdResidence = new Residence
        {
            Id = 1,
            Number = requestDto.Number,
            Floor = requestDto.Floor,
            Tower = requestDto.Tower
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<Residence>());

        _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>()))
            .ReturnsAsync(createdResidence);

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Vivienda creada exitosamente", result.Message);
        Assert.Equal(requestDto.Number, result.Number);
        Assert.Equal(requestDto.Floor, result.Floor);
        Assert.Equal(requestDto.Tower, result.Tower);
        _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
    }

    [Fact]
    public async Task Create_EmptyTower_ReturnsFailureResponse()
    {
        // Arrange
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = ""
        };

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("La torre no puede estar vacía", result.Message);
        _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Never);
    }

    [Fact]
    public async Task Create_NullTower_ReturnsFailureResponse()
    {
        // Arrange
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = null
        };

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("La torre no puede estar vacía", result.Message);
        _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhitespaceTower_ReturnsFailureResponse()
    {
        // Arrange
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = "   "
        };

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

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
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = "A"
        };

        var existingResidences = new List<Residence>
        {
            new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            }
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(existingResidences);

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

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
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = "a"
        };

        var existingResidences = new List<Residence>
        {
            new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            }
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(existingResidences);

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

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
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 2,
            Tower = "A"
        };

        var existingResidences = new List<Residence>
        {
            new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            }
        };

        var createdResidence = new Residence
        {
            Id = 2,
            Number = requestDto.Number,
            Floor = requestDto.Floor,
            Tower = requestDto.Tower
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(existingResidences);

        _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>()))
            .ReturnsAsync(createdResidence);

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(requestDto.Number, result.Number);
        Assert.Equal(requestDto.Floor, result.Floor);
        Assert.Equal(requestDto.Tower, result.Tower);
        _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
    }

    [Fact]
    public async Task Create_SameNumberDifferentTower_ReturnsSuccessResponse()
    {
        // Arrange
        var requestDto = new ResidenceRequestDto
        {
            Number = 101,
            Floor = 1,
            Tower = "B"
        };

        var existingResidences = new List<Residence>
        {
            new Residence
            {
                Id = 1,
                Number = 101,
                Floor = 1,
                Tower = "A"
            }
        };

        var createdResidence = new Residence
        {
            Id = 2,
            Number = requestDto.Number,
            Floor = requestDto.Floor,
            Tower = requestDto.Tower
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(existingResidences);

        _mockResidenceRepository.Setup(repo => repo.Create(It.IsAny<Residence>()))
            .ReturnsAsync(createdResidence);

        // Act
        var result = await _createResidenceUseCase.Create(requestDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(requestDto.Number, result.Number);
        Assert.Equal(requestDto.Floor, result.Floor);
        Assert.Equal(requestDto.Tower, result.Tower);
        _mockResidenceRepository.Verify(repo => repo.Create(It.IsAny<Residence>()), Times.Once);
    }

    #endregion

    #region GetResidenceById Tests

    [Fact]
    public async Task GetResidenceById_ExistingResidence_ReturnsSuccessResponse()
    {
        // Arrange
        var residenceId = 1;
        var residence = new Residence
        {
            Id = residenceId,
            Number = 101,
            Floor = 1,
            Tower = "A"
        };

        _mockResidenceRepository.Setup(repo => repo.GetById(residenceId))
            .ReturnsAsync(residence);

        // Act
        var result = await _createResidenceUseCase.GetResidenceById(residenceId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Vivienda obtenida exitosamente", result.Message);
        Assert.Equal(residence.Number, result.Number);
        Assert.Equal(residence.Floor, result.Floor);
        Assert.Equal(residence.Tower, result.Tower);
        _mockResidenceRepository.Verify(repo => repo.GetById(residenceId), Times.Once);
    }

    [Fact]
    public async Task GetResidenceById_NonExistingResidence_ReturnsFailureResponse()
    {
        // Arrange
        var residenceId = 999;

        _mockResidenceRepository.Setup(repo => repo.GetById(residenceId))
            .ReturnsAsync((Residence?)null);

        // Act
        var result = await _createResidenceUseCase.GetResidenceById(residenceId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Vivienda no encontrada", result.Message);
        _mockResidenceRepository.Verify(repo => repo.GetById(residenceId), Times.Once);
    }

    #endregion

    #region GetAllResidences Tests

    [Fact]
    public async Task GetAllResidences_WithResidences_ReturnsListOfResponses()
    {
        // Arrange
        var residences = new List<Residence>
        {
            new Residence { Id = 1, Number = 101, Floor = 1, Tower = "A" },
            new Residence { Id = 2, Number = 102, Floor = 1, Tower = "A" },
            new Residence { Id = 3, Number = 201, Floor = 2, Tower = "B" }
        };

        _mockResidenceRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(residences);

        // Act
        var result = await _createResidenceUseCase.GetAllResidences();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.True(r.Success));
        Assert.Equal(101, result[0].Number);
        Assert.Equal(102, result[1].Number);
        Assert.Equal(201, result[2].Number);
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