using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Polls
{
    public class GetPollWithResultsTests
    {
        private readonly Mock<IPollRepository> _mockPollRepo;
        private readonly GetPollWithResults _useCase;

        public GetPollWithResultsTests()
        {
            _mockPollRepo = new Mock<IPollRepository>();
            _useCase = new GetPollWithResults(_mockPollRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnPollWithOptionsAndVotes_WhenPollExists()
        {
            int pollId = 1;

            var poll = new Poll
            {
                Id = pollId,
                Title = "Encuesta de mantenimiento",
                Description = "¿Desea pintar el edificio?",
                PollOptions = new List<PollOption>
                {
                    new PollOption
                    {
                        Id = 10,
                        Text = "Sí",
                        Votes = new List<Vote>
                        {
                            new Vote { Id = 1, User = new User { Id = 1 }, PollOption_id = 10 },
                            new Vote { Id = 2, User = new User { Id = 2 }, PollOption_id = 10 }
                        }
                    },
                    new PollOption
                    {
                        Id = 11,
                        Text = "No",
                        Votes = new List<Vote>
                        {
                            new Vote { Id = 3, User = new User { Id = 3 }, PollOption_id = 11 }
                        }
                    }
                },
                ResultPoll = new ResultPoll
                {
                    Id = 50,
                    Description = "Ganó el SÍ",
                    Percentage = 66.6
                }
            };

            _mockPollRepo
                .Setup(r => r.GetPollWithResultsAsync(pollId))
                .ReturnsAsync(poll);

            var result = await _useCase.ExecuteAsync(pollId);
            Assert.NotNull(result);
            Assert.Equal(pollId, result!.Id);
            Assert.Equal("Encuesta de mantenimiento", result.Title);
            Assert.Equal(2, result.PollOptions.Count);

            var optionYes = result.PollOptions.First(o => o.Id == 10);
            Assert.Equal(2, optionYes.Votes.Count);

            var optionNo = result.PollOptions.First(o => o.Id == 11);
            Assert.Equal(1, optionNo.Votes.Count);

            Assert.NotNull(result.ResultPoll);
            Assert.Equal("Ganó el SÍ", result.ResultPoll.Description);

            _mockPollRepo.Verify(r => r.GetPollWithResultsAsync(pollId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNull_WhenPollDoesNotExist()
        {
            int pollId = 999;

            _mockPollRepo
                .Setup(r => r.GetPollWithResultsAsync(pollId))
                .ReturnsAsync((Poll?)null);

            var result = await _useCase.ExecuteAsync(pollId);

            Assert.Null(result);
            _mockPollRepo.Verify(r => r.GetPollWithResultsAsync(pollId), Times.Once);
        }
    }
}
