using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
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
            var poll = new Poll
            {
                Title = "Encuesta de prueba",
                Description = "Descripción de la encuesta",
                CategoryPoll_id = 1,
                User_id = 123,
                State = "Activa",
                PollOptions = new List<PollOption>
                {
                    new PollOption { Text = "Opción 1" },
                    new PollOption { Text = "Opción 2" }
                }
            };

            var user = new User { Id = 123, Name = "Usuario Prueba" };

            _userRepoMock.Setup(repo => repo.GetById(poll.User_id)).ReturnsAsync(user);
            _pollRepoMock.Setup(repo => repo.CreatePoll(It.IsAny<Poll>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _createPoll.ExecuteAsync(poll);

            Assert.NotNull(result);
            Assert.Equal(poll.Title, result.Title);
            Assert.Equal(poll.Description, result.Description);
            Assert.Equal(poll.User_id, result.User_id);
            Assert.Equal("Activa", result.State);
            Assert.Equal(2, result.PollOptions.Count);

            _pollRepoMock.Verify(repo => repo.CreatePoll(It.IsAny<Poll>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var poll = new Poll
            {
                Title = "Encuesta inválida",
                Description = "No debería crearse",
                CategoryPoll_id = 2,
                User_id = 999,
                State = "Activa",
                PollOptions = new List<PollOption>
                {
                    new PollOption { Text = "A" },
                    new PollOption { Text = "B" }
                }
            };

            _userRepoMock.Setup(repo => repo.GetById(poll.User_id)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _createPoll.ExecuteAsync(poll));

            Assert.Equal($"El usuario con ID {poll.User_id} no existe.", ex.Message);

            _pollRepoMock.Verify(repo => repo.CreatePoll(It.IsAny<Poll>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }
    }
}
