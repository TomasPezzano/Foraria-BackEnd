using Moq;
using FluentAssertions;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;


namespace ForariaTest.Unit.Thread
{
    public class CloseThreadTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldCloseThread_WhenThreadExistsAndIsActive()
        {
            // Arrange
            int threadId = 1;

            var thread = new global::ForariaDomain.Thread
            {
                Id = threadId,
                Theme = "Tema de prueba",
                Description = "Descripción del hilo",
                State = "Active",
                UserId = 1,
                ForumId = 2,
                CreatedAt = DateTime.UtcNow
            };

            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetById(threadId)).ReturnsAsync(thread);

            var useCase = new CloseThread(mockRepo.Object);

            // Act
            var result = await useCase.ExecuteAsync(threadId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(threadId);
            result.State.Should().Be("Closed");
            result.Theme.Should().Be("Tema de prueba");
            result.ForumId.Should().Be(2);
            result.UserId.Should().Be(1);

            thread.State.Should().Be("Closed");

            mockRepo.Verify(r => r.UpdateAsync(thread), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenThreadDoesNotExist()
        {
            // Arrange
            int threadId = 999;
            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetById(threadId))
                    .ReturnsAsync((global::ForariaDomain.Thread?)null);

            var useCase = new CloseThread(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(threadId);

            // Assert
            await act.Should().ThrowAsync<ForariaDomain.Exceptions.NotFoundException>()
                .WithMessage($"No se encontró el hilo con ID {threadId}");

            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Thread>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenThreadAlreadyClosed()
        {
            // Arrange
            int threadId = 2;
            var thread = new global::ForariaDomain.Thread
            {
                Id = threadId,
                State = "Closed",
                Theme = "Hilo ya cerrado"
            };

            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetById(threadId)).ReturnsAsync(thread);

            var useCase = new CloseThread(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(threadId);

            // Assert
            await act.Should().ThrowAsync<ForariaDomain.Exceptions.ThreadLockedException>()
                .WithMessage("El hilo ya se encuentra cerrado.");

            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Thread>()), Times.Never);
        }
    }
}
