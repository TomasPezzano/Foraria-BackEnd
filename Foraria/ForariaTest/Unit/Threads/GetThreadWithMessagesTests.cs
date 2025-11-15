using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Threads
{
    public class GetThreadWithMessagesTests
    {
        private readonly Mock<IThreadRepository> _mockRepo;
        private readonly GetThreadWithMessages _useCase;

        public GetThreadWithMessagesTests()
        {
            _mockRepo = new Mock<IThreadRepository>();
            _useCase = new GetThreadWithMessages(_mockRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnThreadWithMessages_WhenThreadExists()
        {
            int threadId = 1;
            var thread = new ForariaDomain.Thread
            {
                Id = threadId,
                Messages = new List<ForariaDomain.Message>
                {
                    new ForariaDomain.Message { Id = 1, Content = "Mensaje 1" },
                    new ForariaDomain.Message { Id = 2, Content = "Mensaje 2" },
                }
            };

            _mockRepo.Setup(r => r.GetByIdWithMessagesAsync(threadId))
                     .ReturnsAsync(thread);

            var result = await _useCase.ExecuteAsync(threadId);

            Assert.NotNull(result);
            Assert.Equal(threadId, result.Id);
            Assert.Equal(2, result.Messages.Count);
            _mockRepo.Verify(r => r.GetByIdWithMessagesAsync(threadId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenThreadDoesNotExist()
        {
            int threadId = 999;

            _mockRepo.Setup(r => r.GetByIdWithMessagesAsync(threadId))
                     .ReturnsAsync((ForariaDomain.Thread?)null);

            Func<Task> act = async () => await _useCase.ExecuteAsync(threadId);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal($"No se encontró el hilo con ID {threadId}", exception.Message);
            _mockRepo.Verify(r => r.GetByIdWithMessagesAsync(threadId), Times.Once);
        }
    }
}
