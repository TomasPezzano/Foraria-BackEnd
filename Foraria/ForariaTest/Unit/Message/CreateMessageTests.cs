using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Moq;
using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ForariaTest.Unit.Message
{
    public class CreateMessageTests
    {
        [Fact]
        public async Task Execute_ShouldCreateMessage_WhenThreadAndUserExist_AndContentIsValid()
        {
            // Arrange
            var request = new CreateMessageWithFileRequest
            {
                Thread_id = 1,
                User_id = 1,
                Content = "Hola mundo",
                FilePath = "test.txt"
            };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 1 };

            var message = new global::ForariaDomain.Message
            {
                Id = 10,
                Content = request.Content,
                Thread_id = request.Thread_id,
                User_id = request.User_id
            };

            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);
            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockMsgRepo.Setup(r => r.Add(It.IsAny<global::ForariaDomain.Message>()))
                       .ReturnsAsync(message);

            var useCase = new CreateMessage(mockMsgRepo.Object, mockThreadRepo.Object, mockUserRepo.Object);

            // Act
            var result = await useCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be("Hola mundo");
            result.User_id.Should().Be(1);
            result.Thread_id.Should().Be(1);

            mockMsgRepo.Verify(r => r.Add(It.IsAny<global::ForariaDomain.Message>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenThreadDoesNotExist()
        {
            var request = new CreateMessageWithFileRequest { Thread_id = 99, User_id = 1, Content = "Hola" };

            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockThreadRepo.Setup(r => r.GetById(99))
                          .ReturnsAsync((global::ForariaDomain.Thread?)null);

            var useCase = new CreateMessage(mockMsgRepo.Object, mockThreadRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El hilo con ID 99 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenUserDoesNotExist()
        {
            var request = new CreateMessageWithFileRequest { Thread_id = 1, User_id = 55, Content = "Hola" };

            var thread = new global::ForariaDomain.Thread { Id = 1 };

            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);
            mockUserRepo.Setup(r => r.GetById(55))
                        .ReturnsAsync((global::ForariaDomain.User?)null);

            var useCase = new CreateMessage(mockMsgRepo.Object, mockThreadRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El usuario con ID 55 no existe.");
        }

        [Fact]
        public async Task Execute_ShouldThrow_WhenContentIsEmpty()
        {
            var request = new CreateMessageWithFileRequest { Thread_id = 1, User_id = 1, Content = "   " };

            var thread = new global::ForariaDomain.Thread { Id = 1 };
            var user = new global::ForariaDomain.User { Id = 1 };

            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);
            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);

            var useCase = new CreateMessage(mockMsgRepo.Object, mockThreadRepo.Object, mockUserRepo.Object);

            Func<Task> act = async () => await useCase.Execute(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El contenido del mensaje no puede estar vacío.");
        }
    }
}
