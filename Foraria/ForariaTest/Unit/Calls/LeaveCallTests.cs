using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class LeaveCallTests
    {
        [Fact]
        public void Execute_ShouldDisconnectParticipant_WhenCallExists()
        {
            // Arrange
            int callId = 1;
            int userId = 42;

            var call = new Call
            {
                Id = callId,
                Status = "Active",
                CreatedByUserId = 1,
                StartedAt = DateTime.UtcNow
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns(call);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new LeaveCall(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            useCase.Execute(callId, userId);

            // Assert
            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockParticipantRepo.Verify(
                r => r.SetDisconnected(callId, userId),
                Times.Once
            );
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;
            int userId = 42;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new LeaveCall(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, userId);

            // Assert
            act.Should()
               .Throw<NotFoundException>()
               .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockParticipantRepo.Verify(
                r => r.SetDisconnected(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never
            );
        }
    }
}
