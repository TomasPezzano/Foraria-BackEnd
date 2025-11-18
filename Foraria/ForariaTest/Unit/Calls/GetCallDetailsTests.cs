using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit.CallTests
{
    public class GetCallDetailsTests
    {
        [Fact]
        public void Execute_ShouldReturnCall_WhenCallExists()
        {
            // Arrange
            int callId = 10;

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 2,
                StartedAt = DateTime.Now,
                Status = "Active"
            };

            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.GetById(callId))
                .Returns(call);

            var useCase = new GetCallDetails(mockRepo.Object);

            // Act
            var result = useCase.Execute(callId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(callId);
            result.Status.Should().Be("Active");
            result.CreatedByUserId.Should().Be(2);

            mockRepo.Verify(r => r.GetById(callId), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;
            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.GetById(callId))
                .Returns((Call?)null);

            var useCase = new GetCallDetails(mockRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId);

            // Assert
            act.Should()
                .Throw<NotFoundException>()
                .WithMessage("La llamada no existe.");

            mockRepo.Verify(r => r.GetById(callId), Times.Once);
        }
    }
}