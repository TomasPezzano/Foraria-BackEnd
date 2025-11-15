using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Threads
{
    public class GetAllThreadsTests
    {
        private readonly Mock<IThreadRepository> _mockRepo;
        private readonly GetAllThreads _useCase;

        public GetAllThreadsTests()
        {
            _mockRepo = new Mock<IThreadRepository>();
            _useCase = new GetAllThreads(_mockRepo.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnAllThreads_WhenNoForumIdProvided()
        {
            var threadsMock = new List<ForariaDomain.Thread>
            {
                new ForariaDomain.Thread { Id = 1, Theme = "Thread 1" },
                new ForariaDomain.Thread { Id = 2, Theme = "Thread 2" }
            };

            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(threadsMock);

            var result = await _useCase.ExecuteAsync();

           
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnThreadsByForumId()
        {
            int forumId = 10;
            var threadsMock = new List<ForariaDomain.Thread>
            {
                new ForariaDomain.Thread { Id = 1, Theme = "Thread A", ForumId = forumId }
            };

            _mockRepo
                .Setup(repo => repo.GetByForumIdAsync(forumId))
                .ReturnsAsync(threadsMock);

            var result = await _useCase.ExecuteAsync(forumId);

           
            Assert.Single(result);
            Assert.Equal(forumId, result.First().ForumId);
            _mockRepo.Verify(repo => repo.GetByForumIdAsync(forumId), Times.Once);
        }

    
        [Fact]
        public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenNoThreadsFound()
        {
            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<ForariaDomain.Thread>()); // vacío

            await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync());

            _mockRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
