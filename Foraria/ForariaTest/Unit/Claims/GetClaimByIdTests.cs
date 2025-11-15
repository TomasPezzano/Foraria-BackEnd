using System.Threading.Tasks;
using Xunit;
using Moq;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Claims
{
    public class GetClaimByIdTests
    {
        private readonly Mock<IClaimRepository> _mockRepo;
        private readonly GetClaimById _useCase;

        public GetClaimByIdTests()
        {
            _mockRepo = new Mock<IClaimRepository>();
            _useCase = new GetClaimById(_mockRepo.Object);
        }

       
        [Fact]
        public async Task Execute_ShouldReturnClaim_WhenClaimExists()
        {
            var claimId = 1;
            var claimMock = new Claim { Id = claimId, Title = "Test Claim" };

            _mockRepo
                .Setup(repo => repo.GetById(claimId))
                .ReturnsAsync(claimMock);

            var result = await _useCase.Execute(claimId);

            Assert.NotNull(result);
            Assert.Equal(claimId, result.Id);
            Assert.Equal("Test Claim", result.Title);

            _mockRepo.Verify(repo => repo.GetById(claimId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenClaimDoesNotExist()
        {
            var claimId = 99;

            _mockRepo
                .Setup(repo => repo.GetById(claimId))
                .ReturnsAsync((Claim)null);

            var result = await _useCase.Execute(claimId);

            Assert.Null(result);
            _mockRepo.Verify(repo => repo.GetById(claimId), Times.Once);
        }
    }
}
