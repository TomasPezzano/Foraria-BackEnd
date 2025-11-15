using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Polls
{
    public class GetPollByIdTests
    {
        private readonly Mock<IPollRepository> _mockRepository;
        private readonly GetPollById _useCase;

        public GetPollByIdTests()
        {
            _mockRepository = new Mock<IPollRepository>();
            _useCase = new GetPollById(_mockRepository.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnPoll_WhenPollExists()
        {
            int pollId = 5;

            var expectedPoll = new Poll
            {
                Id = pollId,
                Title = "Encuesta de mantenimiento",
                Description = "Opinión general sobre el edificio"
            };

            _mockRepository
                .Setup(r => r.GetById(pollId))
                .ReturnsAsync(expectedPoll);

            var result = await _useCase.ExecuteAsync(pollId);

            Assert.NotNull(result);
            Assert.Equal(expectedPoll.Id, result!.Id);
            Assert.Equal(expectedPoll.Title, result.Title);
            Assert.Equal(expectedPoll.Description, result.Description);

            _mockRepository.Verify(r => r.GetById(pollId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNull_WhenPollDoesNotExist()
        {
            int pollId = 999;

            _mockRepository
                .Setup(r => r.GetById(pollId))
                .ReturnsAsync((Poll?)null);

            var result = await _useCase.ExecuteAsync(pollId);

            Assert.Null(result);

            _mockRepository.Verify(r => r.GetById(pollId), Times.Once);
        }
    }
}
