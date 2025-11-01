using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Thread
{
    public class UpdateThreadTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockUserRepo.Setup(r => r.GetById(It.IsAny<int>()))
                        .ReturnsAsync((global::ForariaDomain.User?)null);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var request = new UpdateThreadRequest { UserId = 1 };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(10, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No se encontró el usuario con id 1");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFound_WhenThreadDoesNotExist()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User { Id = 1, Role = new Role { Description = "Administrador" } };
            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(It.IsAny<int>()))
                          .ReturnsAsync((global::ForariaDomain.Thread?)null);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var request = new UpdateThreadRequest { UserId = 1 };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(50, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No se encontró el hilo con id 50");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowForbidden_WhenUserIsNotOwnerOrAdmin()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User
            {
                Id = 2,
                Role = new Role { Description = "Residente" }
            };

            var thread = new global::ForariaDomain.Thread
            {
                Id = 1,
                UserId = 99,
                State = "Active"
            };

            mockUserRepo.Setup(r => r.GetById(2)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(thread);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var request = new UpdateThreadRequest
            {
                UserId = 2,
                Theme = "Nuevo título",
                Description = "Nuevo contenido"
            };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>()
                .WithMessage("No tienes permisos para editar este hilo.");

            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Thread>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateThread_WhenUserIsOwner()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };

            var thread = new global::ForariaDomain.Thread
            {
                Id = 10,
                UserId = 1,
                State = "Active",
                Theme = "Original",
                Description = "Antigua descripción"
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(10)).ReturnsAsync(thread);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var request = new UpdateThreadRequest
            {
                UserId = 1,
                Theme = "Nuevo título",
                Description = "Nueva descripción"
            };

            // Act
            var result = await useCase.ExecuteAsync(10, request);

            // Assert
            result.Theme.Should().Be("Nuevo título");
            result.Description.Should().Be("Nueva descripción");
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Thread>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldAllowAdminToChangeState()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new global::ForariaDomain.User
            {
                Id = 1,
                Role = new Role { Description = "Administrador" }
            };

            var thread = new global::ForariaDomain.Thread
            {
                Id = 100,
                UserId = 99,
                State = "Active"
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(100)).ReturnsAsync(thread);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var request = new UpdateThreadRequest
            {
                UserId = 1,
                State = "Closed"
            };

            // Act
            var result = await useCase.ExecuteAsync(100, request);

            // Assert
            result.State.Should().Be("Closed");
            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<global::ForariaDomain.Thread>()), Times.Once);
        }
    }
}
