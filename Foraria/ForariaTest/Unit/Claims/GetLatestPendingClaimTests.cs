using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Claims
{
    public class GetLatestPendingClaimTests
    {
        private readonly Mock<IClaimRepository> _repositoryMock;
        private readonly GetLatestPendingClaim _useCase;

        public GetLatestPendingClaimTests()
        {
            _repositoryMock = new Mock<IClaimRepository>();
            _useCase = new GetLatestPendingClaim(_repositoryMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNull_WhenNoClaimExists()
        {
            _repositoryMock
                .Setup(r => r.GetLatestPendingAsync())
                .ReturnsAsync((Claim?)null);

            var result = await _useCase.ExecuteAsync();

            Assert.Null(result);
        }

    }
}
