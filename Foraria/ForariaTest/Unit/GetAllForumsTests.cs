using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;


namespace ForariaTest.Unit;

public class GetAllForumsTests
{
    private readonly Mock<IForumRepository> _forumRepositoryMock = new();

    private GetAllForums CreateUseCase()
    {
        return new GetAllForums(_forumRepositoryMock.Object);
    }


    [Fact]
    public async Task Execute_ShouldThrowNotFound_WhenRepositoryReturnsNull()
    {
        _forumRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync((IEnumerable<global::ForariaDomain.Forum>)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => useCase.Execute());

        Assert.Equal("No se encontraron foros disponibles.", ex.Message);
    }

    [Fact]
    public async Task Execute_ShouldThrowNotFound_WhenRepositoryReturnsEmptyList()
    {
        _forumRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new List<global::ForariaDomain.Forum>());

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => useCase.Execute());

        Assert.Equal("No se encontraron foros disponibles.", ex.Message);
    }

    [Fact]
    public async Task Execute_ShouldReturnOnlyActiveForums_WhenThereAreActiveAndInactive()
    {
        var forums = new List<global::ForariaDomain.Forum>
        {
            new global::ForariaDomain.Forum { Id = 1, IsActive = true },
            new global::ForariaDomain.Forum { Id = 2, IsActive = false },
            new global::ForariaDomain.Forum { Id = 3, IsActive = true }
        };

        _forumRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(forums);

        var useCase = CreateUseCase();

        var result = await useCase.Execute();

        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, f => Assert.True(f.IsActive));
        Assert.Contains(resultList, f => f.Id == 1);
        Assert.Contains(resultList, f => f.Id == 3);

        _forumRepositoryMock.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnEmpty_WhenAllForumsInactive()
    {
        var forums = new List<global::ForariaDomain.Forum>
        {
            new global::ForariaDomain.Forum { Id = 1, IsActive = false },
            new global::ForariaDomain.Forum { Id = 2, IsActive = false }
        };

        _forumRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(forums);

        var useCase = CreateUseCase();

        var result = await useCase.Execute();

        Assert.NotNull(result);
        Assert.Empty(result);
        _forumRepositoryMock.Verify(r => r.GetAll(), Times.Once);
    }
}
