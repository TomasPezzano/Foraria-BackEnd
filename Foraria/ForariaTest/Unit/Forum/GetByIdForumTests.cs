using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Foraria.Domain.Model;

namespace ForariaTest.Unit.Forum
{
    public class GetForumByIdTests
    {
        [Fact]
        public async Task Execute_ShouldReturnForumResponse_WhenForumExists()
        {
            // Arrange
            int forumId = 1;

            var mockRepo = new Mock<IForumRepository>();

            var forum = new global::ForariaDomain.Forum
            {
                Id = forumId,
                Category = ForumCategory.General
            };

            mockRepo.Setup(r => r.GetById(forumId))
                    .ReturnsAsync(forum);
            mockRepo.Setup(r => r.TotalThreads(forumId))
                    .ReturnsAsync(5);
            mockRepo.Setup(r => r.TotalResponses(forumId))
                    .ReturnsAsync(20);
            mockRepo.Setup(r => r.TotalUniqueParticipantsIncludingThreadCreators(forumId))
                    .ReturnsAsync(10);

            var useCase = new GetForumById(mockRepo.Object);

            // Act
            var result = await useCase.Execute(forumId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(forumId);
            result.Category.Should().Be(ForumCategory.General);
            result.CategoryName.Should().Be("General");
            result.CountThreads.Should().Be(5);
            result.CountResponses.Should().Be(20);
            result.CountUserActives.Should().Be(10);

            // Verify all repository calls
            mockRepo.Verify(r => r.GetById(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalThreads(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalResponses(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalUniqueParticipantsIncludingThreadCreators(forumId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenForumDoesNotExist()
        {
            // Arrange
            int forumId = 999;

            var mockRepo = new Mock<IForumRepository>();
            mockRepo.Setup(r => r.GetById(forumId))
                    .ReturnsAsync((global::ForariaDomain.Forum?)null);

            var useCase = new GetForumById(mockRepo.Object);

            // Act
            var result = await useCase.Execute(forumId);

            // Assert
            result.Should().BeNull();

            mockRepo.Verify(r => r.GetById(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalThreads(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalResponses(forumId), Times.Once);
            mockRepo.Verify(r => r.TotalUniqueParticipantsIncludingThreadCreators(forumId), Times.Once);
        }
    }
}
