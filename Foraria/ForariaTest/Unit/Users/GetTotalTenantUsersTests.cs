using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.Users
{
    public class GetTotalTenantUsersTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConsortiumRepository> _mockConsortiumRepo;
        private readonly GetTotalTenantUsers _useCase;

        public GetTotalTenantUsersTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConsortiumRepo = new Mock<IConsortiumRepository>();
            _useCase = new GetTotalTenantUsers(_mockUserRepo.Object, _mockConsortiumRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnTotalTenantUsers_WhenConsortiumExists()
        {
            
            int consortiumId = 1;
            int expectedTotal = 8;

            _mockConsortiumRepo.Setup(r => r.FindById(consortiumId))
                               .ReturnsAsync(new ForariaDomain.Consortium { Id = consortiumId });

            _mockUserRepo.Setup(r => r.GetTotalUsersByTenantIdAsync(consortiumId))
                         .ReturnsAsync(expectedTotal);

            var result = await _useCase.ExecuteAsync(consortiumId);

            Assert.Equal(expectedTotal, result);
            _mockConsortiumRepo.Verify(r => r.FindById(consortiumId), Times.Once);
            _mockUserRepo.Verify(r => r.GetTotalUsersByTenantIdAsync(consortiumId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenConsortiumDoesNotExist()
        {
            int consortiumId = 999;

            _mockConsortiumRepo.Setup(r => r.FindById(consortiumId))
                               .ReturnsAsync((ForariaDomain.Consortium?)null);

            Func<Task> act = async () => await _useCase.ExecuteAsync(consortiumId);

         
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            Assert.Equal($"El consorcio con ID {consortiumId} no existe.", exception.Message);
            _mockConsortiumRepo.Verify(r => r.FindById(consortiumId), Times.Once);
            _mockUserRepo.Verify(r => r.GetTotalUsersByTenantIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
