using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Foraria.Domain.Model;

namespace ForariaTest.Unit.Forum
{
    public class CreateForumTests
    {
        [Fact]
        public async Task Execute_ShouldCreateForum_WhenCategoryDoesNotExist()
        {
            // Arrange
            var request = new CreateForumRequest
            {
                Category = ForumCategory.General
            };

            var newForum = new global::ForariaDomain.Forum
            {
                Id = 1,
                Category = ForumCategory.General
            };

            var mockRepo = new Mock<IForumRepository>();
            mockRepo.Setup(r => r.GetByCategory(ForumCategory.General))
                    .ReturnsAsync((global::ForariaDomain.Forum?)null);
            mockRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()))
                    .ReturnsAsync(newForum);

            var useCase = new CreateForum(mockRepo.Object);

            // Act
            var result = await useCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Category.Should().Be(ForumCategory.General);

            mockRepo.Verify(r => r.GetByCategory(ForumCategory.General), Times.Once);
            mockRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowInvalidOperationException_WhenCategoryAlreadyExists()
        {
            // Arrange
            var request = new CreateForumRequest
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
            Func<Task> act = async () => await useCase.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Ya existe un foro para la categoría 'General'.");

            mockRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Forum>()), Times.Never);
        }
    }
}
