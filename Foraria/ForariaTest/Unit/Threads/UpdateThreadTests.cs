using Moq;
using FluentAssertions;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit.Threads
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
                        .ReturnsAsync((User?)null);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var thread = new ForariaDomain.Thread { UserId = 1 };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(10, thread);

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

            var user = new User { Id = 1, Role = new Role { Description = "Administrador" } };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(It.IsAny<int>()))
                          .ReturnsAsync((ForariaDomain.Thread?)null);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var thread = new ForariaDomain.Thread { UserId = 1 };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(50, thread);

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

            var user = new User
            {
                Id = 2,
                Role = new Role { Description = "Residente" }
            };

            var threadDb = new ForariaDomain.Thread
            {
                Id = 1,
                UserId = 99,
                State = "Active"
            };

            mockUserRepo.Setup(r => r.GetById(2)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(1)).ReturnsAsync(threadDb);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var update = new ForariaDomain.Thread
            {
                UserId = 2,
                Theme = "Nuevo título",
                Description = "Nuevo contenido"
            };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(1, update);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>()
                .WithMessage("No tienes permisos para editar este hilo.");

            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<ForariaDomain.Thread>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateThread_WhenUserIsOwner()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };

            var threadDb = new ForariaDomain.Thread
            {
                Id = 10,
                UserId = 1,
                State = "Active",
                Theme = "Original",
                Description = "Antigua descripción"
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(10)).ReturnsAsync(threadDb);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var update = new ForariaDomain.Thread
            {
                UserId = 1,
                Theme = "Nuevo título",
                Description = "Nueva descripción"
            };

            // Act
            var result = await useCase.ExecuteAsync(10, update);

            // Assert
            result.Theme.Should().Be("Nuevo título");
            result.Description.Should().Be("Nueva descripción");
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<ForariaDomain.Thread>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldAllowAdminToChangeState()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new User
            {
                Id = 1,
                Role = new Role { Description = "Administrador" }
            };

            var threadDb = new ForariaDomain.Thread
            {
                Id = 100,
                UserId = 99,
                State = "Active"
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(100)).ReturnsAsync(threadDb);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var update = new ForariaDomain.Thread
            {
                UserId = 1,
                State = "Closed"
            };

            // Act
            var result = await useCase.ExecuteAsync(100, update);

            // Assert
            result.State.Should().Be("Closed");
            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<ForariaDomain.Thread>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowForbidden_WhenNonAdminTriesToChangeState()
        {
            // Arrange
            var mockThreadRepo = new Mock<IThreadRepository>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new User
            {
                Id = 1,
                Role = new Role { Description = "Residente" }
            };

            var threadDb = new ForariaDomain.Thread
            {
                Id = 100,
                UserId = 1,
                State = "Active"
            };

            mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);
            mockThreadRepo.Setup(r => r.GetById(100)).ReturnsAsync(threadDb);

            var useCase = new UpdateThread(mockThreadRepo.Object, mockUserRepo.Object);

            var update = new ForariaDomain.Thread
            {
                UserId = 1,
                State = "Closed"
            };

            // Act
            Func<Task> act = async () => await useCase.ExecuteAsync(100, update);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>()
                .WithMessage("No tienes permisos para cambiar el estado del hilo.");

            mockThreadRepo.Verify(r => r.UpdateAsync(It.IsAny<ForariaDomain.Thread>()), Times.Never);
        }
    }
}
