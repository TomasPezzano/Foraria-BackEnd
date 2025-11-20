using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Models;
using Moq;

namespace ForariaTest.Unit
{
    public class CreateVoteTests
    {
        private readonly Mock<IVoteRepository> _voteRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISignalRNotification> _signalRNotificationMock;
        private readonly Mock<IGetPollById> _getPollById;
        private readonly CreateVote _createVote;

        public CreateVoteTests()
        {
            _voteRepoMock = new Mock<IVoteRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _signalRNotificationMock = new Mock<ISignalRNotification>();
            _getPollById = new Mock<IGetPollById>();

            _createVote = new CreateVote(
                _voteRepoMock.Object,
                _unitOfWorkMock.Object,
                _userRepoMock.Object,
                _signalRNotificationMock.Object,
                _getPollById.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCreateVote_WhenUserExistsAndHasNotVoted()
        {
            var vote = new Vote { User_id = 1, Poll_id = 10, PollOption_id = 100 };
            var user = new User { Id = vote.User_id };
            var poll = new Poll { Id = vote.Poll_id, State = "Activa" };

            _userRepoMock.Setup(r => r.GetById(vote.User_id)).ReturnsAsync(user);
            _getPollById.Setup(g => g.ExecuteAsync(vote.Poll_id)).ReturnsAsync(poll);
            _voteRepoMock.Setup(r => r.GetByUserAndPollAsync(vote.User_id, vote.Poll_id)).ReturnsAsync((Vote?)null);
            _voteRepoMock.Setup(r => r.AddAsync(It.IsAny<Vote>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _voteRepoMock.Setup(r => r.GetPollResultsAsync(vote.Poll_id)).ReturnsAsync(new List<PollResult>());
            _signalRNotificationMock.Setup(n => n.NotifyPollUpdatedAsync(It.IsAny<int>(), It.IsAny<IEnumerable<PollResult>>()))
                                     .Returns(Task.CompletedTask);

            await _createVote.ExecuteAsync(vote);

            _voteRepoMock.Verify(r => r.AddAsync(It.Is<Vote>(v => v.User_id == vote.User_id)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _signalRNotificationMock.Verify(n => n.NotifyPollUpdatedAsync(vote.Poll_id, It.IsAny<IEnumerable<PollResult>>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var vote = new Vote { User_id = 2, Poll_id = 10, PollOption_id = 100 };
            _userRepoMock.Setup(r => r.GetById(vote.User_id)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _createVote.ExecuteAsync(vote));

            Assert.Equal($"El usuario con ID {vote.User_id} no existe.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyVoted()
        {
            var vote = new Vote { User_id = 3, Poll_id = 20, PollOption_id = 200 };
            var user = new User { Id = vote.User_id };
            var existingVote = new Vote { User_id = vote.User_id, Poll_id = vote.Poll_id };
            var poll = new Poll { Id = vote.Poll_id, State = "Activa" };

            _userRepoMock.Setup(r => r.GetById(vote.User_id)).ReturnsAsync(user);
            _getPollById.Setup(g => g.ExecuteAsync(vote.Poll_id)).ReturnsAsync(poll);
            _voteRepoMock.Setup(r => r.GetByUserAndPollAsync(vote.User_id, vote.Poll_id)).ReturnsAsync(existingVote);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _createVote.ExecuteAsync(vote));
            Assert.Equal("El usuario ya votó en esta encuesta.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenPollDoesNotExist()
        {
            var vote = new Vote { User_id = 1, Poll_id = 99, PollOption_id = 100 };
            var user = new User { Id = vote.User_id };

            _userRepoMock.Setup(r => r.GetById(vote.User_id)).ReturnsAsync(user);
            _getPollById.Setup(g => g.ExecuteAsync(vote.Poll_id)).ReturnsAsync((Poll?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _createVote.ExecuteAsync(vote));
            Assert.Equal($"La votacion con ID {vote.Poll_id} no existe.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenPollIsFinalizadaOrPendiente()
        {
            var vote = new Vote { User_id = 1, Poll_id = 5, PollOption_id = 100 };
            var user = new User { Id = vote.User_id };
            var poll = new Poll { Id = vote.Poll_id, State = "Finalizada" };

            _userRepoMock.Setup(r => r.GetById(vote.User_id)).ReturnsAsync(user);
            _getPollById.Setup(g => g.ExecuteAsync(vote.Poll_id)).ReturnsAsync(poll);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _createVote.ExecuteAsync(vote));
            Assert.Equal("No se puede votar en una votacion en estado pendiente o finalizada", ex.Message);
        }
    }
}
