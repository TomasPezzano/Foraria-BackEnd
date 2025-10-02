using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain.Exceptions;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit
{
    public class CreatePollTests
    {
        private readonly Mock<IPollRepository> _pollRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly CreatePoll _createPoll;

        public CreatePollTests()
        {
            _pollRepoMock = new Mock<IPollRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _createPoll = new CreatePoll(
                _pollRepoMock.Object,
                _unitOfWorkMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCreatePoll_WhenUserExists()
        {
            var request = new PollDto
            {
                Title = "Encuesta de prueba",
                Description = "Descripción de la encuesta",
                CategoryPollId = 1,
                UserId = 123,
                Options = new List<string> { "Opción 1", "Opción 2" }
            };

            var user = new User { Id = 123, Name = "Usuario Prueba" };

            _userRepoMock.Setup(repo => repo.GetById(request.UserId)).ReturnsAsync(user);
            _pollRepoMock.Setup(repo => repo.CreatePoll(It.IsAny<Poll>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);
            var result = await _createPoll.ExecuteAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.UserId, result.User_id);
            Assert.Equal("Activa", result.State);
            Assert.Equal(2, result.PollOptions.Count);

            _pollRepoMock.Verify(repo => repo.CreatePoll(It.IsAny<Poll>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var request = new PollDto
            {
                Title = "Encuesta inválida",
                Description = "No debería crearse",
                CategoryPollId = 2,
                UserId = 999,
                Options = new List<string> { "A", "B" }
            };

            _userRepoMock.Setup(repo => repo.GetById(request.UserId)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _createPoll.ExecuteAsync(request));

            Assert.Equal("El usuario con ID 999 no existe.", ex.Message);

            _pollRepoMock.Verify(repo => repo.CreatePoll(It.IsAny<Poll>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }
    }
}
