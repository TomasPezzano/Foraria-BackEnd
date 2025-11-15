using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain;
using Moq;

namespace ForariaTest.Unit.UserDocuments
{
    public class GetUserDocumentsByCategoryTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldReturnDocuments_WhenRepositoryReturnsData()
        {
            string category = "CategoryA";
            int? userId = 1;

            var expectedDocuments = new List<UserDocument>
            {
                new UserDocument { Id = 1, Title = "Doc1", Category = category, CreatedAt = DateTime.Now },
                new UserDocument { Id = 2, Title = "Doc2", Category = category, CreatedAt = DateTime.Now }
            };

            var repositoryMock = new Mock<IUserDocumentRepository>();
            repositoryMock
                .Setup(repo => repo.GetByCategoryAsync(category, userId))
                .ReturnsAsync(expectedDocuments);

            var getUserDocumentsByCategory = new GetUserDocumentsByCategory(repositoryMock.Object);

            var result = await getUserDocumentsByCategory.ExecuteAsync(category, userId);

            Assert.NotNull(result);
            Assert.Equal(expectedDocuments.Count, result.Count);
            Assert.All(result, doc => Assert.Equal(category, doc.Category));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallRepositoryWithNullUserId_WhenUserIdNotProvided()
        {
            string category = "CategoryB";

            var repositoryMock = new Mock<IUserDocumentRepository>();
            repositoryMock
                .Setup(repo => repo.GetByCategoryAsync(category, null))
                .ReturnsAsync(new List<UserDocument>());

            var getUserDocumentsByCategory = new GetUserDocumentsByCategory(repositoryMock.Object);

            var result = await getUserDocumentsByCategory.ExecuteAsync(category);

            repositoryMock.Verify(repo => repo.GetByCategoryAsync(category, null), Times.Once);
            Assert.NotNull(result);
        }
    }
}
