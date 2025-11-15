using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Moq;
using Xunit;

namespace ForariaTest.Unit.Users
{
    public class GetTotalUsersTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsTotalUsers_WhenConsortiumIdIsProvided()
        {
            var mockRepository = new Mock<IUserRepository>();
            int consortiumId = 1;
            int expectedTotalUsers = 5;

            mockRepository
                .Setup(r => r.GetTotalUsersAsync(consortiumId))
                .ReturnsAsync(expectedTotalUsers);

            var useCase = new GetTotalUsers(mockRepository.Object);

            var result = await useCase.ExecuteAsync(consortiumId);

            Assert.Equal(expectedTotalUsers, result);
            mockRepository.Verify(r => r.GetTotalUsersAsync(consortiumId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsTotalUsers_WhenConsortiumIdIsNull()
        {
            var mockRepository = new Mock<IUserRepository>();
            int expectedTotalUsers = 10;

            mockRepository
                .Setup(r => r.GetTotalUsersAsync(null))
                .ReturnsAsync(expectedTotalUsers);

            var useCase = new GetTotalUsers(mockRepository.Object);

            var result = await useCase.ExecuteAsync();

            Assert.Equal(expectedTotalUsers, result);
            mockRepository.Verify(r => r.GetTotalUsersAsync(null), Times.Once);
        }
    }
}

