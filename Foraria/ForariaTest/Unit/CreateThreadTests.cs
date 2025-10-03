using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Moq;
using Thread = ForariaDomain.Thread;

namespace ForariaTest.Application;

public class CreateThreadTests
{
    private readonly Mock<IThreadRepository> _mockRepo;
    private readonly CreateThread _useCase;

    public CreateThreadTests()
    {
        _mockRepo = new Mock<IThreadRepository>();
        _useCase = new CreateThread(_mockRepo.Object);
    }

    [Fact]
    public async Task GivenValidThreadRequest_WhenExecutingCreateThread_ThenReturnsThreadResponse()
    {
        // Given
        var request = new CreateThreadRequest
        {
            Theme = "prueba",
            Description = "descripcion",
            Forum_id = 1,
            User_id = 10
        };

        var thread = new Thread
        {
            Id = 1,
            Theme = request.Theme,
            Description = request.Description,
            Forum_id = request.Forum_id,
            User_id = request.User_id,
            CreatedAt = DateTime.UtcNow,
            State = "Active"
        };

        _mockRepo.Setup(r => r.Add(It.IsAny<Thread>()))
            .Returns(Task.CompletedTask)
            .Callback<Thread>(t => t.Id = 1); 

        // When
        var result = await _useCase.Execute(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(request.Theme, result.Theme);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Forum_id, result.Forum_id);
        Assert.Equal(request.User_id, result.User_id);
        _mockRepo.Verify(r => r.Add(It.IsAny<Thread>()), Times.Once);
    }
}
