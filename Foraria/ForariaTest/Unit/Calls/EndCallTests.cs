using System;
using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Repository;
using Foraria.Application.UseCase;
using ForariaDomain.Exceptions;

namespace ForariaTest.Unit.CallTests
{
    public class EndCallTests
    {
        [Fact]
        public void Execute_ShouldEndCall_WhenCallExists()
        {
            // Arrange
            int callId = 5;
            var existingCall = new Call
            {
                Id = callId,
                CreatedByUserId = 1,
                StartedAt = DateTime.UtcNow,
                Status = "Active"
            };

            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.GetById(callId))
                .Returns(existingCall);

            var useCase = new EndCall(mockRepo.Object);

            // Act
            useCase.Execute(callId);

            // Assert
            existingCall.Status.Should().Be("Ended");
            existingCall.EndedAt.Should().NotBeNull();
            existingCall.EndedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            mockRepo.Verify(r => r.GetById(callId), Times.Once);
            mockRepo.Verify(r => r.Update(existingCall), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 99;
            var mockRepo = new Mock<ICallRepository>();

            mockRepo
                .Setup(r => r.GetById(callId))
                .Returns((Call?)null);

            var useCase = new EndCall(mockRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId);

            // Assert
            act.Should()
                .Throw<NotFoundException>()
                .WithMessage("Llamada no encontrada");

            mockRepo.Verify(r => r.Update(It.IsAny<Call>()), Times.Never);
        }
    }
}
