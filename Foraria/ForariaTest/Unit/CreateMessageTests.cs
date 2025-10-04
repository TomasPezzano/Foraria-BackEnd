using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Thread = ForariaDomain.Thread;

namespace ForariaTest.Application
{
    public class CreateMessageTests
    {
        private readonly Mock<IMessageRepository> _mockMessageRepo;
        private readonly Mock<IThreadRepository> _mockThreadRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly CreateMessage _useCase;

        public CreateMessageTests()
        {
            _mockMessageRepo = new Mock<IMessageRepository>();
            _mockThreadRepo = new Mock<IThreadRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _useCase = new CreateMessage(
                _mockMessageRepo.Object,
                _mockThreadRepo.Object,
                _mockUserRepo.Object,
                Mock.Of<IWebHostEnvironment>()
            );
        }

        [Fact]
        public async Task GivenValidMessageRequest_WhenExecutingCreateMessage_ThenReturnsMessage()
        {
            // Given
            var request = new CreateMessageWithFileRequest
            {
                Content = "mensaje de prueba",
                Thread_id = 1,
                User_id = 99,
                File = null
            };

            var thread = new Thread { Id = 1 };
            var user = new User { Id = 99 };

            _mockThreadRepo.Setup(r => r.GetById(request.Thread_id)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(request.User_id)).ReturnsAsync(user);

            var createdMessage = new Message
            {
                Id = 1,
                Content = request.Content,
                Thread_id = request.Thread_id,
                User_id = request.User_id,
                CreatedAt = DateTime.UtcNow,
                State = "active"
            };

            _mockMessageRepo.Setup(r => r.Add(It.IsAny<Message>())).ReturnsAsync(createdMessage);

            // When
            var result = await _useCase.Execute(request);

            // Then
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(request.Content, result.Content);
            Assert.Equal(request.Thread_id, result.Thread_id);
            Assert.Equal(request.User_id, result.User_id);
            Assert.Equal("active", result.State);

            _mockThreadRepo.Verify(r => r.GetById(request.Thread_id), Times.Once);
            _mockUserRepo.Verify(r => r.GetById(request.User_id), Times.Once);
            _mockMessageRepo.Verify(r => r.Add(It.IsAny<Message>()), Times.Once);
        }
    }
}
