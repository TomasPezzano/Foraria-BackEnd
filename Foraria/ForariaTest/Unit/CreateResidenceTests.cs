using Moq;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaTest.Unit;
public class CreateResidenceTests
{
    private readonly Mock<IResidenceRepository> _residenceRepositoryMock = new();
    private readonly Mock<IConsortiumRepository> _consortiumRepositoryMock = new();

    private CreateResidence CreateUseCase()
    {
        return new CreateResidence(
            _residenceRepositoryMock.Object,
            _consortiumRepositoryMock.Object
        );
    }


    [Fact]
    public async Task Create_ShouldReturnError_WhenTowerIsEmpty()
    {
        var useCase = CreateUseCase();

        var result = await useCase.Create(1, 101, 2, "");

        Assert.False(result.Success);
        Assert.Equal("La torre no puede estar vacía", result.Message);
        Assert.Null(result.Residence);
    }


    [Fact]
    public async Task Create_ShouldReturnError_WhenNumberIsZero()
    {
        var useCase = CreateUseCase();

        var result = await useCase.Create(1, 0, 2, "A");

        Assert.False(result.Success);
        Assert.Equal("El número no puede estar vacío", result.Message);
        Assert.Null(result.Residence);
    }


    [Fact]
    public async Task Create_ShouldReturnError_WhenConsortiumDoesNotExist()
    {
        _consortiumRepositoryMock
            .Setup(x => x.FindById(99))
            .ReturnsAsync((global::ForariaDomain.Consortium)null);

        var useCase = CreateUseCase();

        var result = await useCase.Create(99, 101, 2, "A");

        Assert.False(result.Success);
        Assert.Equal("El consorcio con ID 99 no existe", result.Message);
        Assert.Null(result.Residence);
    }


    [Fact]
    public async Task Create_ShouldReturnError_WhenResidenceAlreadyExists()
    {
        var consortium = new global::ForariaDomain.Consortium { Id = 1 };

        _consortiumRepositoryMock
            .Setup(x => x.FindById(1))
            .ReturnsAsync(consortium);

        var existingResidences = new List<global::ForariaDomain.Residence>
        {
            new global::ForariaDomain.Residence
            {
                Number = 101,
                Floor = 2,
                Tower = "A"
            }
        };

        _residenceRepositoryMock
            .Setup(x => x.GetResidenceByConsortiumIdAsync(1))
            .ReturnsAsync(existingResidences);

        var useCase = CreateUseCase();

        var result = await useCase.Create(1, 101, 2, "A");

        Assert.False(result.Success);
        Assert.Equal("Ya existe una vivienda con ese número, piso y torre", result.Message);
        Assert.Null(result.Residence);
    }

    [Fact]
    public async Task Create_ShouldCreateResidenceSuccessfully()
    {
        var consortium = new global::ForariaDomain.Consortium { Id = 1 };

        _consortiumRepositoryMock
            .Setup(x => x.FindById(1))
            .ReturnsAsync(consortium);

        _residenceRepositoryMock
            .Setup(x => x.GetResidenceByConsortiumIdAsync(1))
            .ReturnsAsync(new List<global::ForariaDomain.Residence>());  // No residencias existentes

        var createdResidence = new global::ForariaDomain.Residence
        {
            Id = 999,
            Number = 101,
            Floor = 2,
            Tower = "A",
            ConsortiumId = 1
        };

        _residenceRepositoryMock
            .Setup(x => x.Create(It.IsAny<global::ForariaDomain.Residence>(), 1))
            .ReturnsAsync(createdResidence);

        var useCase = CreateUseCase();

        var result = await useCase.Create(1, 101, 2, "A");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Vivienda creada exitosamente", result.Message);
        Assert.NotNull(result.Residence);
        Assert.Equal(999, result.Residence.Id);

        // Se verifica que Create haya sido llamado correctamente
        _residenceRepositoryMock.Verify(
            x => x.Create(It.IsAny<global::ForariaDomain.Residence>(), 1),
            Times.Once
        );
    }
}
