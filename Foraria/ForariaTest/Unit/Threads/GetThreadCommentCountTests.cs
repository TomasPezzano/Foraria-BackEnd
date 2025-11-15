using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Threads
{
    public class GetThreadCommentCountTests
    {
        private readonly Mock<IThreadRepository> _mockRepo;
        private readonly GetThreadCommentCount _useCase;

        public GetThreadCommentCountTests()
        {
            _mockRepo = new Mock<IThreadRepository>();
            _useCase = new GetThreadCommentCount(_mockRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnMessageCount_WhenThreadHasMessages()
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

            _mockRepo.Setup(r => r.GetById(threadId))
                     .ReturnsAsync(thread);

            var count = await _useCase.Execute(threadId);

            Assert.Equal(2, count);
            _mockRepo.Verify(r => r.GetById(threadId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnZero_WhenThreadHasNoMessages()
        {
            int threadId = 2;
            var thread = new ForariaDomain.Thread
            {
                Id = threadId,
                Messages = new List<ForariaDomain.Message>() 
            };

            _mockRepo.Setup(r => r.GetById(threadId))
                     .ReturnsAsync(thread);

            var count = await _useCase.Execute(threadId);

            Assert.Equal(0, count);
            _mockRepo.Verify(r => r.GetById(threadId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowException_WhenThreadDoesNotExist()
        {
            int threadId = 999;

            _mockRepo.Setup(r => r.GetById(threadId))
                     .ReturnsAsync((ForariaDomain.Thread?)null);

            Func<Task> act = async () => await _useCase.Execute(threadId);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal($"No se encontró el hilo con ID {threadId}", exception.Message);
            _mockRepo.Verify(r => r.GetById(threadId), Times.Once);
        }
    }
}
