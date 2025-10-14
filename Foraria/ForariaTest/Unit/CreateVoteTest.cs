using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Moq;

namespace ForariaTest.Unit
{
    public class CreateVoteTests
    {
        private readonly Mock<IVoteRepository> _voteRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISignalRNotification> _signalRNotificationMock;
        private readonly CreateVote _createVote;

        public CreateVoteTests()
        {
            _voteRepoMock = new Mock<IVoteRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _signalRNotificationMock = new Mock<ISignalRNotification>();

            _createVote = new CreateVote(
                _voteRepoMock.Object,
                _unitOfWorkMock.Object,
                _userRepoMock.Object,
                _signalRNotificationMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCreateVote_WhenUserExistsAndHasNotVoted()
        {
            var vote = new Vote
            {
                User_id = 1,
                Poll_id = 10,
                PollOption_id = 100
            };

            var user = new User { Id = vote.User_id };

            _userRepoMock.Setup(repo => repo.GetById(vote.User_id))
                         .ReturnsAsync(user);

            _voteRepoMock.Setup(repo => repo.GetByUserAndPollAsync(vote.User_id, vote.Poll_id))
                         .ReturnsAsync((Vote?)null);

            _voteRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Vote>()))
                         .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .ReturnsAsync(1);

            _voteRepoMock.Setup(repo => repo.GetPollResultsAsync(vote.Poll_id))
               .ReturnsAsync(new List<PollResultDto>
               {
                 new() { PollOptionId = 100, VotesCount = 1 }
               });


            _signalRNotificationMock.Setup(n =>
                    n.NotifyPollUpdatedAsync(It.IsAny<int>(), It.IsAny<IEnumerable<PollResultDto>>()))
                .Returns(Task.CompletedTask);

            await _createVote.ExecuteAsync(vote);

            _voteRepoMock.Verify(repo => repo.AddAsync(It.Is<Vote>(v =>
                v.User_id == vote.User_id &&
                v.Poll_id == vote.Poll_id &&
                v.PollOption_id == vote.PollOption_id)), Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);

            _signalRNotificationMock.Verify(n =>
                n.NotifyPollUpdatedAsync(vote.Poll_id, It.IsAny<IEnumerable<PollResultDto>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var vote = new Vote
            {
                User_id = 2,
                Poll_id = 10,
                PollOption_id = 100
            };

            _userRepoMock.Setup(repo => repo.GetById(vote.User_id))
                         .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _createVote.ExecuteAsync(vote));

            Assert.Equal($"El usuario con ID {vote.User_id} no existe.", ex.Message);

            _voteRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Vote>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
            _signalRNotificationMock.Verify(n =>
                n.NotifyPollUpdatedAsync(It.IsAny<int>(), It.IsAny<IEnumerable<PollResultDto>>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowInvalidOperationException_WhenUserAlreadyVoted()
        {
            var vote = new Vote
            {
                User_id = 3,
                Poll_id = 20,
                PollOption_id = 200
            };

            var user = new User { Id = vote.User_id };
            var existingVote = new Vote { User_id = vote.User_id, Poll_id = vote.Poll_id };

            _userRepoMock.Setup(repo => repo.GetById(vote.User_id)).ReturnsAsync(user);
            _voteRepoMock.Setup(repo => repo.GetByUserAndPollAsync(vote.User_id, vote.Poll_id))
                         .ReturnsAsync(existingVote);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _createVote.ExecuteAsync(vote));

            Assert.Equal("El usuario ya votó en esta encuesta.", ex.Message);

            _voteRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Vote>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Never);
            _signalRNotificationMock.Verify(n =>
                n.NotifyPollUpdatedAsync(It.IsAny<int>(), It.IsAny<IEnumerable<PollResultDto>>()), Times.Never);
        }
    }
}
