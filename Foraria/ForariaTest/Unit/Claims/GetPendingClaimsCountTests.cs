using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Claims
{
    public class GetPendingClaimsCountTests
    {
        private readonly Mock<IClaimRepository> _mockClaimRepo;
        private readonly GetPendingClaimsCount _useCase;

        public GetPendingClaimsCountTests()
        {
            _mockClaimRepo = new Mock<IClaimRepository>();
            _useCase = new GetPendingClaimsCount(_mockClaimRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnCount_WhenConsortiumIdProvided()
        {
            int consortiumId = 5;
            int expected = 12;

            _mockClaimRepo
                .Setup(r => r.GetPendingCountAsync(consortiumId))
                .ReturnsAsync(expected);

            var result = await _useCase.ExecuteAsync(consortiumId);

            Assert.Equal(expected, result);
            _mockClaimRepo.Verify(r => r.GetPendingCountAsync(consortiumId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnCount_WhenConsortiumIdIsNull()
        {
            int expected = 7;

            _mockClaimRepo
                .Setup(r => r.GetPendingCountAsync(null))
                .ReturnsAsync(expected);

            var result = await _useCase.ExecuteAsync(null);

            Assert.Equal(expected, result);
            _mockClaimRepo.Verify(r => r.GetPendingCountAsync(null), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZero_WhenRepositoryReturnsZero()
        {
            _mockClaimRepo
                .Setup(r => r.GetPendingCountAsync(It.IsAny<int?>()))
                .ReturnsAsync(0);

            var result = await _useCase.ExecuteAsync(1);

            Assert.Equal(0, result);
        }
    }
}
