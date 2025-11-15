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
    public class GetTotalOwnerUsersTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConsortiumRepository> _mockConsortiumRepo;
        private readonly GetTotalOwnerUsers _useCase;

        public GetTotalOwnerUsersTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConsortiumRepo = new Mock<IConsortiumRepository>();
            _useCase = new GetTotalOwnerUsers(_mockUserRepo.Object, _mockConsortiumRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnTotalOwnerUsers_WhenConsortiumExists()
        {
            int consortiumId = 1;
            int expectedTotal = 5;

            _mockConsortiumRepo.Setup(r => r.FindById(consortiumId))
                               .ReturnsAsync(new ForariaDomain.Consortium { Id = consortiumId });

            _mockUserRepo.Setup(r => r.GetTotalOwnerUsersAsync(consortiumId))
                         .ReturnsAsync(expectedTotal);

            var result = await _useCase.ExecuteAsync(consortiumId);

            Assert.Equal(expectedTotal, result);
            _mockConsortiumRepo.Verify(r => r.FindById(consortiumId), Times.Once);
            _mockUserRepo.Verify(r => r.GetTotalOwnerUsersAsync(consortiumId), Times.Once);
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
            _mockUserRepo.Verify(r => r.GetTotalOwnerUsersAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
