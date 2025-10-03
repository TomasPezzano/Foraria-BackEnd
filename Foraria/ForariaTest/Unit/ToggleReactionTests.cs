using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain;
using Moq;

namespace ForariaTest.Application;

public class ToggleReactionTests
{
    private readonly Mock<IReactionRepository> _mockRepo;
    private readonly ToggleReaction _useCase;

    public ToggleReactionTests()
    {
        _mockRepo = new Mock<IReactionRepository>();
        _useCase = new ToggleReaction(_mockRepo.Object);
    }

    [Fact]
    public async Task GivenNoExistingReaction_WhenTogglingLike_ThenAddsLikeAndReturnsTrue()
    {
        // Given
        _mockRepo.Setup(r => r.GetByUserAndTarget(1, 10, null))
            .ReturnsAsync((Reaction?)null);

        // When
        var result = await _useCase.Execute(1, 10, null, 1);

        // Then
        Assert.True(result);
        _mockRepo.Verify(r => r.Add(It.IsAny<Reaction>()), Times.Once);
    }

    [Fact]
    public async Task GivenExistingLike_WhenTogglingSameLike_ThenRemovesReactionAndReturnsFalse()
    {
        // Given
        var existing = new Reaction { User_id = 1, Message_id = 10, ReactionType = 1 };

        _mockRepo.Setup(r => r.GetByUserAndTarget(1, 10, null))
            .ReturnsAsync(existing);

        // When
        var result = await _useCase.Execute(1, 10, null, 1);

        // Then
        Assert.False(result);
        _mockRepo.Verify(r => r.Remove(existing), Times.Once);
    }

    [Fact]
    public async Task GivenExistingLike_WhenTogglingDislike_ThenReplacesReactionAndReturnsTrue()
    {
        // Given
        var existing = new Reaction { User_id = 1, Message_id = 10, ReactionType = 1 };

        _mockRepo.Setup(r => r.GetByUserAndTarget(1, 10, null))
            .ReturnsAsync(existing);

        // When
        var result = await _useCase.Execute(1, 10, null, -1);

        // Then
        Assert.True(result);
        _mockRepo.Verify(r => r.Remove(existing), Times.Once);
        _mockRepo.Verify(r => r.Add(It.IsAny<Reaction>()), Times.Once);
    }
}
