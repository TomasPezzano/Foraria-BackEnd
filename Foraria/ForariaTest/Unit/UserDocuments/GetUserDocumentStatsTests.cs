using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
using Moq;

namespace ForariaTest.Unit.UserDocuments
{
    public class GetUserDocumentStatsTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldReturnStats_WhenRepositoryReturnsData()
        {
            int? userId = 1;
            var expectedStats = new UserDocumentStatsResult
            {
                TotalUserDocuments = 5,
                TotalConsortiumDocuments = 3,
                TotalCombined = 8,
                DocumentsByCategory = new Dictionary<string, int>
                {
                    { "CategoryA", 2 },
                    { "CategoryB", 3 }
                },
                LastUploadDate = new DateTime(2025, 11, 15)
            };

            var repositoryMock = new Mock<IUserDocumentRepository>();
            repositoryMock
                .Setup(repo => repo.GetStatsAsync(userId))
                .ReturnsAsync(expectedStats);

            var getUserDocumentStats = new GetUserDocumentStats(repositoryMock.Object);

            
            var result = await getUserDocumentStats.ExecuteAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(expectedStats.TotalUserDocuments, result.TotalUserDocuments);
            Assert.Equal(expectedStats.TotalConsortiumDocuments, result.TotalConsortiumDocuments);
            Assert.Equal(expectedStats.TotalCombined, result.TotalCombined);
            Assert.Equal(expectedStats.DocumentsByCategory, result.DocumentsByCategory);
            Assert.Equal(expectedStats.LastUploadDate, result.LastUploadDate);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallRepositoryWithNull_WhenNoUserIdProvided()
        {
            var repositoryMock = new Mock<IUserDocumentRepository>();
            repositoryMock
                .Setup(repo => repo.GetStatsAsync(null))
                .ReturnsAsync(new UserDocumentStatsResult());

            var getUserDocumentStats = new GetUserDocumentStats(repositoryMock.Object);

            var result = await getUserDocumentStats.ExecuteAsync();

            repositoryMock.Verify(repo => repo.GetStatsAsync(null), Times.Once);
            Assert.NotNull(result);
        }
    }
}
