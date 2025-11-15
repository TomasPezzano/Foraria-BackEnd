using System.Threading.Tasks;
using Xunit;
using Moq;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using ForariaDomain.Repository;

namespace ForariaTest.Unit
{
    public class GetConsortiumByIdTests
    {
        private readonly Mock<IConsortiumRepository> _mockRepo;
        private readonly GetConsortiumById _useCase;

        public GetConsortiumByIdTests()
        {
            _mockRepo = new Mock<IConsortiumRepository>();
            _useCase = new GetConsortiumById(_mockRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnConsortium_WhenConsortiumExists()
        {
            int consortiumId = 1;

            var consortiumMock = new Consortium
            {
                Id = consortiumId,
                Name = "Consorcio Test"
            };

            _mockRepo
                .Setup(repo => repo.FindById(consortiumId))
                .ReturnsAsync(consortiumMock);

            var result = await _useCase.Execute(consortiumId);

            Assert.NotNull(result);
            Assert.Equal(consortiumId, result.Id);
            Assert.Equal("Consorcio Test", result.Name);

            _mockRepo.Verify(repo => repo.FindById(consortiumId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenConsortiumDoesNotExist()
        {
            int consortiumId = 999;

            _mockRepo
                .Setup(repo => repo.FindById(consortiumId))
                .ReturnsAsync((Consortium)null);

            var result = await _useCase.Execute(consortiumId);

            Assert.Null(result);
            _mockRepo.Verify(repo => repo.FindById(consortiumId), Times.Once);
        }
    }
}
