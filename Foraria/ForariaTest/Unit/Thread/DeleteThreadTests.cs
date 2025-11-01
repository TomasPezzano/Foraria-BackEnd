using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Tests.Thread
{
    public class DeleteThreadTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldDeleteThread_WhenThreadExistsAndHasNoMessages()
        {
            // Arrange
            int threadId = 1;

            var thread = new global::ForariaDomain.Thread
            {
                Id = threadId,
                Messages = new List<global::ForariaDomain.Message>() // sin mensajes
            };

            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetByIdWithMessagesAsync(threadId))
                    .ReturnsAsync(thread);

            var useCase = new DeleteThread(mockRepo.Object);

            // Act
            await useCase.ExecuteAsync(threadId);

            // Assert
            mockRepo.Verify(r => r.GetByIdWithMessagesAsync(threadId), Times.Once);
            mockRepo.Verify(r => r.DeleteAsync(threadId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenThreadDoesNotExist()
        {
            // Arrange
            int threadId = 999;

            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetByIdWithMessagesAsync(threadId))
                    .ReturnsAsync((global::ForariaDomain.Thread?)null);

            var useCase = new DeleteThread(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(threadId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No se encontró el thread con ID 999");

            mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenThreadHasMessages()
        {
            // Arrange
            int threadId = 5;

            var thread = new global::ForariaDomain.Thread
            {
                Id = threadId,
                Messages = new List<global::ForariaDomain.Message>
                {
                    new global::ForariaDomain.Message { Id = 1, Content = "Primer mensaje" },
                    new global::ForariaDomain.Message { Id = 2, Content = "Segundo mensaje" }
                }
            };

            var mockRepo = new Mock<IThreadRepository>();
            mockRepo.Setup(r => r.GetByIdWithMessagesAsync(threadId))
                    .ReturnsAsync(thread);

            var useCase = new DeleteThread(mockRepo.Object);

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(threadId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No se puede eliminar un thread que contiene mensajes.");

            mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
