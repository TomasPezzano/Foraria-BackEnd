using System;
using FluentAssertions;
using Moq;
using ForariaDomain;
using Foraria.Application.UseCase;
using ForariaDomain.Repository;
using Xunit;

namespace ForariaTest.Unit.Calls
{
    public class CreateCallTests
    {
        [Fact]
        public void Execute_ShouldCreateCall_WithCorrectUserId()
        {
            // Arrange
            int userId = 10;

            var createdCall = new Call
            {
                Id = 1,
                CreatedByUserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = "Active"
            };

            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.Create(It.IsAny<Call>()))
                .Returns(createdCall);

            var useCase = new CreateCall(mockRepo.Object);

            // Act
            var result = useCase.Execute(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CreatedByUserId.Should().Be(userId);
            result.Status.Should().Be("Active");

            result.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            mockRepo.Verify(r => r.Create(It.IsAny<Call>()), Times.Once);
        }
    }
}
