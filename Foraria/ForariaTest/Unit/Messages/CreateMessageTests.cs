using FluentAssertions;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Messages
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
        public async Task Execute_ShouldCreateMessage_WhenValidData()
        {
            var message = new global::ForariaDomain.Message
            {
                Content = "mensaje de prueba",
                Thread_id = 1,
                User_id = 99
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 99 };

            _mockThreadRepo.Setup(r => r.GetById(message.Thread_id)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(message.User_id)).ReturnsAsync(user);

            var createdMessage = new global::ForariaDomain.Message
            {
                Id = 1,
                Content = message.Content,
                Thread_id = message.Thread_id,
                User_id = message.User_id,
                CreatedAt = DateTime.UtcNow,
                State = "active"
            };

            _mockMessageRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Message>()))
                            .ReturnsAsync(createdMessage);

            var result = await _useCase.Execute(message);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Content.Should().Be("mensaje de prueba");
            result.Thread_id.Should().Be(1);
            result.User_id.Should().Be(99);
            result.State.Should().Be("active");

            _mockThreadRepo.Verify(r => r.GetById(1), Times.Once);
            _mockUserRepo.Verify(r => r.GetById(99), Times.Once);
            _mockMessageRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Message>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenThreadDoesNotExist()
        {
            var message = new global::ForariaDomain.Message
            {
                Content = "mensaje",
                Thread_id = 999,
                User_id = 1
            };

            _mockThreadRepo.Setup(r => r.GetById(999))
                           .ReturnsAsync((global::ForariaDomain.Thread?)null);

            Func<Task> act = async () => await _useCase.Execute(message);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El hilo con ID 999 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenUserDoesNotExist()
        {
            var message = new global::ForariaDomain.Message
            {
                Content = "mensaje",
                Thread_id = 1,
                User_id = 999
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };

            _mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(999))
                         .ReturnsAsync((global::ForariaDomain.User?)null);

            Func<Task> act = async () => await _useCase.Execute(message);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El usuario con ID 999 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenContentIsEmpty()
        {
            var message = new global::ForariaDomain.Message
            {
                Content = "  ",
                Thread_id = 1,
                User_id = 1
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 1 };

            _mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);
            _mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);

            Func<Task> act = async () => await _useCase.Execute(message);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El contenido del mensaje no puede estar vacío.");
        }
    }
}
