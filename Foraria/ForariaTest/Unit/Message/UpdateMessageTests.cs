using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ForariaTest.Unit.Message
{
    public class UpdateMessageTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockUserRepo.Setup(r => r.GetById(It.IsAny<int>()))
                        .ReturnsAsync((global::ForariaDomain.User?)null);

            var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);

            var request = new UpdateMessageRequest { UserId = 1 };

            Func<Task> act = async () => await useCase.ExecuteAsync(1, request);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No se encontró el usuario con id 1");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFound_WhenMessageDoesNotExist()
        {
            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User { Id = 1, Role = new Role { Description = "admin" } };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockMsgRepo.Setup(r => r.GetById(1))
                       .ReturnsAsync((global::ForariaDomain.Message?)null);

            var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
            var request = new UpdateMessageRequest { UserId = 1 };

            Func<Task> act = async () => await useCase.ExecuteAsync(1, request);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No se encontró el mensaje con id 1");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperation_WhenMessageIsDeleted()
        {
            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User { Id = 1, Role = new Role { Description = "admin" } };
            var msg = new global::ForariaDomain.Message { Id = 1, IsDeleted = true };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockMsgRepo.Setup(r => r.GetById(1)).ReturnsAsync(msg);

            var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
            var request = new UpdateMessageRequest { UserId = 1 };

            Func<Task> act = async () => await useCase.ExecuteAsync(1, request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No se puede editar un mensaje eliminado.");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateContent_WhenOwnerEditsWithin15Minutes()
        {
            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "residente" }
            };

            var msg = new global::ForariaDomain.Message
            {
                Id = 10,
                User_id = 1,
                Content = "Viejo",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                IsDeleted = false
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

            var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
            var request = new UpdateMessageRequest
            {
                UserId = 1,
                Content = "Nuevo contenido"
            };

            var result = await useCase.ExecuteAsync(10, request);

            result.Content.Should().Be("Nuevo contenido");
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            mockMsgRepo.Verify(r => r.Update(It.IsAny<global::ForariaDomain.Message>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowForbidden_WhenOwnerEditsAfter15Minutes()
        {
            var mockMsgRepo = new Mock<IMessageRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "residente" }
            };

            var msg = new global::ForariaDomain.Message
            {
                Id = 10,
                User_id = 1,
                Content = "Viejo",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                IsDeleted = false
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockMsgRepo.Setup(r => r.GetById(10)).ReturnsAsync(msg);

            var useCase = new UpdateMessage(mockMsgRepo.Object, mockUserRepo.Object);
            var request = new UpdateMessageRequest { UserId = 1, Content = "Tarde" };

            Func<Task> act = async () => await useCase.ExecuteAsync(10, request);

            await act.Should().ThrowAsync<ForbiddenAccessException>()
                .WithMessage("Solo puedes editar tu mensaje dentro de los primeros 15 minutos.");
        }
    }
}
