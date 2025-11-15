using FluentAssertions;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Messages
{
    public class GetMessagesByUserTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldReturnMappedMessages()
        {
            // Arrange
            int userId = 1;
            var messages = new List<global::ForariaDomain.Message>
            {
                new global::ForariaDomain.Message { Id = 1, Content = "Hola", CreatedAt = DateTime.UtcNow, State = "active", User_id = userId },
                new global::ForariaDomain.Message { Id = 2, Content = "Adiós", CreatedAt = DateTime.UtcNow, State = "active", User_id = userId }
            };

            var mockRepo = new Mock<IMessageRepository>();
            mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(messages);

            var useCase = new GetMessagesByUser(mockRepo.Object);

            // Act
            var result = (await useCase.ExecuteAsync(userId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Content.Should().Be("Hola");
            result[1].Id.Should().Be(2);
            result[1].Content.Should().Be("Adiós");

            mockRepo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnEmptyList_WhenUserHasNoMessages()
        {
            var mockRepo = new Mock<IMessageRepository>();
            mockRepo.Setup(r => r.GetByUserIdAsync(1))
                    .ReturnsAsync(Enumerable.Empty<global::ForariaDomain.Message>());

            var useCase = new GetMessagesByUser(mockRepo.Object);

            var result = await useCase.ExecuteAsync(1);

            result.Should().BeEmpty();
        }
    }
}
