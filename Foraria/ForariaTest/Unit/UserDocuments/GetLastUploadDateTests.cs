using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;

namespace ForariaTest.Unit.UserDocuments
{
    public class GetLastUploadDateTests
    {
        private readonly Mock<IUserDocumentRepository> _repositoryMock;
        private readonly GetLastUploadDate _useCase;

        public GetLastUploadDateTests()
        {
            _repositoryMock = new Mock<IUserDocumentRepository>();
            _useCase = new GetLastUploadDate(_repositoryMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnDate_WhenUserIdProvided()
        {
            int userId = 5;
            DateTime expectedDate = new DateTime(2024, 5, 1);

            _repositoryMock
                .Setup(r => r.GetLastUploadDateAsync(userId))
                .ReturnsAsync(expectedDate);

            var result = await _useCase.ExecuteAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(expectedDate, result);
            _repositoryMock.Verify(r => r.GetLastUploadDateAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnDate_WhenUserIdIsNull()
        {
            DateTime expectedDate = new DateTime(2024, 6, 15);

            _repositoryMock
                .Setup(r => r.GetLastUploadDateAsync(null))
                .ReturnsAsync(expectedDate);

            var result = await _useCase.ExecuteAsync(null);

            Assert.NotNull(result);
            Assert.Equal(expectedDate, result);
            _repositoryMock.Verify(r => r.GetLastUploadDateAsync(null), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            _repositoryMock
                .Setup(r => r.GetLastUploadDateAsync(It.IsAny<int?>()))
                .ReturnsAsync((DateTime?)null);

            var result = await _useCase.ExecuteAsync(1);

            Assert.Null(result);
        }
    }
}
