using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using System.Linq;
using FluentAssertions;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Messages
{
    public class GetMessagesByThreadTests
    {
        private readonly Mock<IMessageRepository> _mockMessageRepo;
        private readonly GetMessagesByThread _useCase;

        public GetMessagesByThreadTests()
        {
            _mockMessageRepo = new Mock<IMessageRepository>();
            _useCase = new GetMessagesByThread(_mockMessageRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnMessages_WhenMessagesExistForThread()
        {
            int threadId = 1;
            var messages = new List<Message>
            {
                new Message { Id = 1, Thread_id = threadId, Content = "Mensaje 1" },
                new Message { Id = 2, Thread_id = threadId, Content = "Mensaje 2" },
                new Message { Id = 3, Thread_id = threadId, Content = "Mensaje 3" }
            };

            _mockMessageRepo.Setup(r => r.GetByThread(threadId))
                            .ReturnsAsync(messages);

            var result = await _useCase.Execute(threadId);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, m => m.Content == "Mensaje 1");
            Assert.Contains(result, m => m.Content == "Mensaje 2");
            Assert.Contains(result, m => m.Content == "Mensaje 3");

            _mockMessageRepo.Verify(r => r.GetByThread(threadId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnEmpty_WhenNoMessagesExistForThread()
        {
         
            int threadId = 99;  
            _mockMessageRepo.Setup(r => r.GetByThread(threadId))
                            .ReturnsAsync(new List<Message>());

            var result = await _useCase.Execute(threadId);

            Assert.NotNull(result);
            Assert.Empty(result);

            _mockMessageRepo.Verify(r => r.GetByThread(threadId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrowException_WhenThreadIdIsInvalid()
        {
            int threadId = -1;  
            _mockMessageRepo.Setup(r => r.GetByThread(threadId))
                            .ThrowsAsync(new InvalidOperationException("ID de hilo inválido"));

            Func<Task> act = async () => await _useCase.Execute(threadId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                      .WithMessage("ID de hilo inválido");
        }
    }
}

