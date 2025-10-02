using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit
{
    public class CreateVoteTests
    {
        private readonly Mock<IVoteRepository> _voteRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateVote _createVote;

        public CreateVoteTests()
        {
            _voteRepoMock = new Mock<IVoteRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _createVote = new CreateVote(
                _voteRepoMock.Object,
                _unitOfWorkMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCreateVote_WhenUserExistsAndHasNotVoted()
        {
            int userId = 1;
            int pollId = 10;
            int optionId = 100;

            var user = new User { Id = userId };
            _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            _voteRepoMock.Setup(repo => repo.GetByUserAndPollAsync(userId, pollId)).ReturnsAsync((Vote?)null);
            _voteRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Vote>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            await _createVote.ExecuteAsync(userId, pollId, optionId);

            _voteRepoMock.Verify(repo => repo.AddAsync(It.Is<Vote>(v =>
                v.User_id == userId &&
                v.Poll_id == pollId &&
                v.PollOption_id == optionId)), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            int userId = 2, pollId = 10, optionId = 100;
            _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _createVote.ExecuteAsync(userId, pollId, optionId));

            Assert.Equal($"El usuario con ID {userId} no existe.", ex.Message);
            _voteRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Vote>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyVoted()
        {
            int userId = 3, pollId = 20, optionId = 200;

            var user = new User { Id = userId };
            var existingVote = new Vote { User_id = userId, Poll_id = pollId };

            _userRepoMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            _voteRepoMock.Setup(repo => repo.GetByUserAndPollAsync(userId, pollId)).ReturnsAsync(existingVote);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _createVote.ExecuteAsync(userId, pollId, optionId));

            Assert.Equal("El usuario ya votó en esta encuesta.", ex.Message);
            _voteRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Vote>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }
    }
}
