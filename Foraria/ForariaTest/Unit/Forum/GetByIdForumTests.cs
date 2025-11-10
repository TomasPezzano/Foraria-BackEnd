using Moq;
using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.Forum;

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
        result.CountThreads.Should().Be(5);
        result.CountResponses.Should().Be(20);
        result.CountUserActives.Should().Be(10);

        mockRepo.Verify(r => r.GetById(forumId), Times.Once);
        mockRepo.Verify(r => r.TotalThreads(forumId), Times.Once);
        mockRepo.Verify(r => r.TotalResponses(forumId), Times.Once);
        mockRepo.Verify(r => r.TotalUniqueParticipantsIncludingThreadCreators(forumId), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowNotFoundException_WhenForumDoesNotExist()
    {
        // Arrange
        int forumId = 999;

        var mockRepo = new Mock<IForumRepository>();
        mockRepo.Setup(r => r.GetById(forumId))
                .ReturnsAsync((global::ForariaDomain.Forum?)null);

        var useCase = new GetForumById(mockRepo.Object);

        // Act
        var act = async () => await useCase.Execute(forumId);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"No se encontró el foro con ID {forumId}.");

        mockRepo.Verify(r => r.GetById(forumId), Times.Once);
        // Como no encuentra el foro, nunca debería llamar a los demás:
        mockRepo.Verify(r => r.TotalThreads(It.IsAny<int>()), Times.Never);
        mockRepo.Verify(r => r.TotalResponses(It.IsAny<int>()), Times.Never);
        mockRepo.Verify(r => r.TotalUniqueParticipantsIncludingThreadCreators(It.IsAny<int>()), Times.Never);
    }
}
