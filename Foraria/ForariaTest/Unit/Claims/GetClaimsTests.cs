using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.Claims
{
    public class GetClaimsTests
    {
        private readonly Mock<IClaimRepository> _mockRepo;
        private readonly GetClaims _useCase;

        public GetClaimsTests()
        {
            _mockRepo = new Mock<IClaimRepository>();
            _useCase = new GetClaims(_mockRepo.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnListOfClaims()
        {
            
            int consortiumId = 5;

            var claimsMock = new List<Claim>
            {
                new Claim { Id = 1, Title = "Claim A", ConsortiumId = consortiumId },
                new Claim { Id = 2, Title = "Claim B", ConsortiumId = consortiumId }
            };

            _mockRepo
                .Setup(repo => repo.GetAll(consortiumId))
                .ReturnsAsync(claimsMock);

            
            var result = await _useCase.Execute(consortiumId);

         
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Claim A", result[0].Title);

            _mockRepo.Verify(repo => repo.GetAll(consortiumId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnEmptyList_WhenNoClaimsExist()
        {
           
            int consortiumId = 3;

            _mockRepo
                .Setup(repo => repo.GetAll(consortiumId))
                .ReturnsAsync(new List<Claim>());

       
            var result = await _useCase.Execute(consortiumId);


            Assert.NotNull(result);
            Assert.Empty(result);

            _mockRepo.Verify(repo => repo.GetAll(consortiumId), Times.Once);
        }
    }
}
