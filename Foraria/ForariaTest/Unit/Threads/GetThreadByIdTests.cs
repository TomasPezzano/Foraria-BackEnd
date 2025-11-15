using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Threads
{
    public class GetThreadByIdTests
    {
        private readonly Mock<IThreadRepository> _mockRepo;
        private readonly GetThreadById _useCase;

        public GetThreadByIdTests()
        {
            _mockRepo = new Mock<IThreadRepository>();
            _useCase = new GetThreadById(_mockRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnThread_WhenThreadExists()
        {
            int threadId = 1;
            var thread = new ForariaDomain.Thread
            {
                Id = threadId,
                Theme = "Hilo de prueba"
            };

            _mockRepo.Setup(r => r.GetById(threadId))
                     .ReturnsAsync(thread);

            var result = await _useCase.Execute(threadId);

            Assert.NotNull(result);
            Assert.Equal(threadId, result.Id);
            Assert.Equal("Hilo de prueba", result.Theme);

            _mockRepo.Verify(r => r.GetById(threadId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowNotFoundException_WhenThreadDoesNotExist()
        {
            int threadId = 999;

            _mockRepo.Setup(r => r.GetById(threadId))
                     .ReturnsAsync((ForariaDomain.Thread?)null);

            Func<Task> act = async () => await _useCase.Execute(threadId);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);
            Assert.Equal($"No se encontró el hilo con ID {threadId}.", exception.Message);

            _mockRepo.Verify(r => r.GetById(threadId), Times.Once);
        }
    }
}
