using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ForariaTest.Tests.Message
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
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task GivenValidMessageRequest_WhenExecutingCreateMessage_ThenReturnsMessage()
        {
            // Arrange
            var request = new CreateMessageWithFileRequest
            {
                Content = "mensaje de prueba",
                Thread_id = 1,
                User_id = 99,
                File = null
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 99 };

            _mockThreadRepo.Setup(r => r.GetById(request.Thread_id)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(request.User_id)).ReturnsAsync(user);

            var createdMessage = new global::ForariaDomain.Message
            {
                Id = 1,
                Content = request.Content,
                Thread_id = request.Thread_id,
                User_id = request.User_id,
                CreatedAt = DateTime.UtcNow,
                State = "active"
            };

            _mockMessageRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Message>()))
                            .ReturnsAsync(createdMessage);

            // Act
            var result = await _useCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Content.Should().Be(request.Content);
            result.Thread_id.Should().Be(request.Thread_id);
            result.User_id.Should().Be(request.User_id);
            result.State.Should().Be("active");

            _mockThreadRepo.Verify(r => r.GetById(request.Thread_id), Times.Once);
            _mockUserRepo.Verify(r => r.GetById(request.User_id), Times.Once);
            _mockMessageRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Message>()), Times.Once);
        }

        [Fact]
        public async Task GivenNonexistentThread_WhenExecutingCreateMessage_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateMessageWithFileRequest
            {
                Content = "mensaje",
                Thread_id = 999,
                User_id = 1
            };

            _mockThreadRepo.Setup(r => r.GetById(request.Thread_id))
                           .ReturnsAsync((global::ForariaDomain.Thread?)null);

            // Act
            Func<Task> act = async () => await _useCase.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El hilo con ID 999 no existe.");
        }

        [Fact]
        public async Task GivenNonexistentUser_WhenExecutingCreateMessage_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateMessageWithFileRequest
            {
                Content = "mensaje",
                Thread_id = 1,
                User_id = 999
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };

            _mockThreadRepo.Setup(r => r.GetById(request.Thread_id)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(request.User_id))
                         .ReturnsAsync((global::ForariaDomain.User?)null);

            // Act
            Func<Task> act = async () => await _useCase.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El usuario con ID 999 no existe.");
        }

        [Fact]
        public async Task GivenEmptyContent_WhenExecutingCreateMessage_ThenThrowsInvalidOperationException()
        {
            // Arrange
            var request = new CreateMessageWithFileRequest
            {
                Content = "  ",
                Thread_id = 1,
                User_id = 1
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 1 };

            _mockThreadRepo.Setup(r => r.GetById(request.Thread_id)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(request.User_id)).ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _useCase.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El contenido del mensaje no puede estar vacío.");
        }
    }
}
