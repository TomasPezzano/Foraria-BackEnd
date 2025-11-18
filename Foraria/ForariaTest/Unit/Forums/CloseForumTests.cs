using Moq;
using FluentAssertions;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Tests.Forums
{
    public class DeleteForumTests
    {
        [Fact]
        public async Task Execute_ShouldDisableForum_WhenValid()
        {
            // Arrange
            int forumId = 1;
            var mockRepo = new Mock<IForumRepository>();

            var threads = new List<global::ForariaDomain.Thread>
            {
                new() { Id = 1, State = "Closed" },
                new() { Id = 2, State = "Archived" }
            };

            var forum = new global::ForariaDomain.Forum
            {
                Id = forumId,
                IsActive = true,
                Threads = threads
            };

            mockRepo.Setup(r => r.GetByIdWithThreadsAsync(forumId))
                    .ReturnsAsync(forum);

            var useCase = new DeleteForum(mockRepo.Object);

            // Act
            await useCase.Execute(forumId);

            // Assert
            forum.IsActive.Should().BeFalse();
            mockRepo.Verify(r => r.UpdateAsync(forum), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFoundException_WhenForumDoesNotExist()
        {
            // Arrange
            int forumId = 999;
            var mockRepo = new Mock<IForumRepository>();
            mockRepo.Setup(r => r.GetByIdWithThreadsAsync(forumId))
                    .ReturnsAsync((global::ForariaDomain.Forum?)null);

            var useCase = new DeleteForum(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.Execute(forumId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"No se encontró el foro con ID {forumId}.");

            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Forum>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldThrowBusinessException_WhenForumAlreadyDisabled()
        {
            // Arrange
            int forumId = 2;
            var mockRepo = new Mock<IForumRepository>();

            var forum = new global::ForariaDomain.Forum
            {
                Id = forumId,
                IsActive = false,
                Threads = []
            };

            mockRepo.Setup(r => r.GetByIdWithThreadsAsync(forumId))
                    .ReturnsAsync(forum);

            var useCase = new DeleteForum(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.Execute(forumId);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("El foro ya está deshabilitado.");

            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Forum>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldThrowBusinessException_WhenForumHasActiveThreads()
        {
            // Arrange
            int forumId = 3;
            var mockRepo = new Mock<IForumRepository>();

            var threads = new List<global::ForariaDomain.Thread>
            {
                new global::ForariaDomain.Thread { Id = 1, State = "Active" },
                new global::ForariaDomain.Thread { Id = 2, State = "Closed" }
            };

            var forum = new global::ForariaDomain.Forum
            {
                Id = forumId,
                IsActive = true,
                Threads = threads
            };

            mockRepo.Setup(r => r.GetByIdWithThreadsAsync(forumId))
                    .ReturnsAsync(forum);

            var useCase = new DeleteForum(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.Execute(forumId);

            // Assert
            await act.Should().ThrowAsync<BusinessException>()
                .WithMessage("No se puede deshabilitar el foro porque contiene threads activos.");

            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Forum>()), Times.Never);
        }
    }
}
