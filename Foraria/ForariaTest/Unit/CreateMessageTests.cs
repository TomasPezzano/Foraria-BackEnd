using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace ForariaTest.Application;

public class CreateMessageTests
{
    private readonly Mock<IMessageRepository> _mockRepo;
    private readonly CreateMessage _useCase;

    public CreateMessageTests()
    {
        _mockRepo = new Mock<IMessageRepository>();
        _useCase = new CreateMessage(_mockRepo.Object, Mock.Of<IWebHostEnvironment>());
    }

    [Fact]
    public async Task GivenValidMessageRequest_WhenExecutingCreateMessage_ThenReturnsMessage()
    {
        // Given
        var request = new CreateMessageWithFileRequest
        {
            Content = "mensaje",
            Thread_id = 1,
            User_id = 99,
            File = null
        };

        var createdMessage = new Message
        {
            Id = 1,
            Content = request.Content,
            Thread_id = request.Thread_id,
            User_id = request.User_id,
            CreatedAt = DateTime.UtcNow,
            State = "active"
        };

        _mockRepo.Setup(r => r.Add(It.IsAny<Message>()))
            .ReturnsAsync(createdMessage);

        // When
        var result = await _useCase.Execute(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(request.Content, result.Content);
        Assert.Equal(request.Thread_id, result.Thread_id);
        Assert.Equal(request.User_id, result.User_id);
        Assert.Equal("active", result.State);
        _mockRepo.Verify(r => r.Add(It.IsAny<Message>()), Times.Once);
    }
}
