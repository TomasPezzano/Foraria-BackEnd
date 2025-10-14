using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit
{
    public class GetPollsTests
    {
        private readonly Mock<IPollRepository> _pollRepoMock;
        private readonly GetPolls _getPolls;

        public GetPollsTests()
        {
            _pollRepoMock = new Mock<IPollRepository>();
            _getPolls = new GetPolls(_pollRepoMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnPollList()
        {
            var polls = new List<Poll>
    {
        new Poll
        {
            Title = "Encuesta 1",
            Description = "Descripción 1",
            CategoryPoll_id = 1,
            User_id = 101,
            PollOptions = new List<PollOption>
            {
                new PollOption { Text = "Opción A" },
                new PollOption { Text = "Opción B" }
            }
        },
        new Poll
        {
            Title = "Encuesta 2",
            Description = "Descripción 2",
            CategoryPoll_id = 2,
            User_id = 102,
            PollOptions = null
        }
    };

            _pollRepoMock.Setup(repo => repo.GetAllPolls()).ReturnsAsync(polls);

            var result = await _getPolls.ExecuteAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var firstPoll = result[0];
            Assert.Equal("Encuesta 1", firstPoll.Title);
            Assert.Equal("Descripción 1", firstPoll.Description);
            Assert.Equal(1, firstPoll.CategoryPoll_id);
            Assert.Equal(101, firstPoll.User_id);
            Assert.Equal(2, firstPoll.PollOptions.Count);

            var secondPoll = result[1];
            Assert.Equal("Encuesta 2", secondPoll.Title);
            Assert.Equal(102, secondPoll.User_id);
            Assert.Null(secondPoll.PollOptions);
        }


    }
}
