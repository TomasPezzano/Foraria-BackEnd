using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Forums
{
    public class GetForumWithCategoryTests
    {
        private readonly Mock<IForumRepository> _repositoryMock;
        private readonly GetForumWithCategory _useCase;

        public GetForumWithCategoryTests()
        {
            _repositoryMock = new Mock<IForumRepository>();
            _useCase = new GetForumWithCategory(_repositoryMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnForum_WhenForumExists_AndHasValidCategory()
        {
            var forum = new Forum
            {
                Id = 10,
                IsActive = true,
                Category = ForumCategory.General
            };

            _repositoryMock
                .Setup(r => r.GetById(10))
                .ReturnsAsync(forum);

            var result = await _useCase.Execute(10);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal(ForumCategory.General, result.Category);
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFoundException_WhenNoForumExists()
        {
            _repositoryMock
                .Setup(r => r.GetById(99))
                .ReturnsAsync((Forum?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(99));
        }



        [Fact]
        public async Task Execute_ShouldReturnForum_WithSpecificCategory()
        {
            var forum = new Forum
            {
                Id = 20,
                Category = ForumCategory.Seguridad
            };

            _repositoryMock
                .Setup(r => r.GetById(20))
                .ReturnsAsync(forum);

            var result = await _useCase.Execute(20);

            Assert.Equal(ForumCategory.Seguridad, result.Category);
        }
    }

}
