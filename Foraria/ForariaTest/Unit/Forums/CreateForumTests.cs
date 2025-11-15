using Moq;
using FluentAssertions;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit.Forums
{
    public class CreateForumTests
    {
        [Fact]
        public async Task Execute_ShouldCreateForum_WhenCategoryDoesNotExist()
        {
            // Arrange
            var forum = new global::ForariaDomain.Forum
            {
                Id = 0,
                Category = ForumCategory.General
            };

            var createdForum = new global::ForariaDomain.Forum
            {
                Id = 1,
                Category = ForumCategory.General
            };

            var mockRepo = new Mock<IForumRepository>();
            mockRepo.Setup(r => r.GetByCategory(ForumCategory.General))
                    .ReturnsAsync((global::ForariaDomain.Forum?)null);
            mockRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()))
                    .ReturnsAsync(createdForum);

            var useCase = new CreateForum(mockRepo.Object);

            // Act
            var result = await useCase.Execute(forum);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Category.Should().Be(ForumCategory.General);

            mockRepo.Verify(r => r.GetByCategory(ForumCategory.General), Times.Once);
            mockRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowBusinessException_WhenCategoryAlreadyExists()
        {
            // Arrange
            var forum = new global::ForariaDomain.Forum
            {
                Category = ForumCategory.General
            };

            var existingForum = new global::ForariaDomain.Forum
            {
                Id = 2,
                Category = ForumCategory.General
            };

            var mockRepo = new Mock<IForumRepository>();
            mockRepo.Setup(r => r.GetByCategory(ForumCategory.General))
                    .ReturnsAsync(existingForum);

            var useCase = new CreateForum(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.Execute(forum);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("Ya existe un foro para la categoría 'General'.");

            mockRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()), Times.Never);
        }
    }
}
