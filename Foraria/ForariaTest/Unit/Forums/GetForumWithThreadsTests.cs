using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Forums
{
    public class GetForumWithThreadsTests
    {
        private readonly Mock<IForumRepository> _repositoryMock;
        private readonly GetForumWithThreads _useCase;

        public GetForumWithThreadsTests()
        {
            _repositoryMock = new Mock<IForumRepository>();
            _useCase = new GetForumWithThreads(_repositoryMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnForumWithThreads_WhenForumExists()
        {
            var forum = new Forum
            {
                Id = 10,
                Category = ForumCategory.General,
                Threads = new List<ForariaDomain.Thread>
            {
                new ForariaDomain.Thread { Id = 1, Theme = "Primer tema" },
                new ForariaDomain.Thread { Id = 2, Theme = "Segundo tema" }
            }
            };

            _repositoryMock
                .Setup(r => r.GetByIdWithThreadsAsync(10))
                .ReturnsAsync(forum);

            var result = await _useCase.Execute(10);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal(ForumCategory.General, result.Category);
            Assert.NotEmpty(result.Threads);
            Assert.Equal(2, result.Threads.Count);
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFoundException_WhenForumDoesNotExist()
        {
            _repositoryMock
                .Setup(r => r.GetByIdWithThreadsAsync(99))
                .ReturnsAsync((Forum?)null);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(99));
            Assert.Contains("No se encontró el foro con ID 99", exception.Message);
        }

        [Fact]
        public async Task Execute_ShouldReturnForumWithNoThreads_WhenRepositoryReturnsEmptyList()
        {
            var forum = new Forum
            {
                Id = 5,
                Category = ForumCategory.Seguridad,
                Threads = new List<ForariaDomain.Thread>()
            };

            _repositoryMock
                .Setup(r => r.GetByIdWithThreadsAsync(5))
                .ReturnsAsync(forum);

            var result = await _useCase.Execute(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal(ForumCategory.Seguridad, result.Category);
            Assert.Empty(result.Threads);
        }
    }
}
