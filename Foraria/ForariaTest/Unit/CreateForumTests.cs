using Foraria.Application.UseCase;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Moq;

namespace ForariaTest.Application;

public class CreateForumTests
{
    private readonly Mock<IForumRepository> _mockRepo;
    private readonly CreateForum _useCase;

    public CreateForumTests()
    {
        _mockRepo = new Mock<IForumRepository>();
        _useCase = new CreateForum(_mockRepo.Object);
    }

    [Fact]
    public async Task GivenValidForumRequest_WhenExecutingCreateForum_ThenReturnsForumResponse()
    {
        // Given
        var request = new CreateForumRequest
        {
            Category = ForumCategory.General
        };

        var createdForum = new Forum
        {
            Id = 1,
            Category = ForumCategory.General
        };

        _mockRepo.Setup(r => r.Add(It.IsAny<Forum>()))
            .ReturnsAsync(createdForum);

        // When
        var result = await _useCase.Execute(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(ForumCategory.General, result.Category);
        _mockRepo.Verify(r => r.Add(It.IsAny<Forum>()), Times.Once);
    }
}