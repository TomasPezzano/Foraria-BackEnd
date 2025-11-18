using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class ToggleMuteTests
    {
        [Fact]
        public void Execute_ShouldToggleMute_WhenCallExists()
        {
            // Arrange
            int callId = 15;
            int userId = 88;
            bool isMuted = true;

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 1,
                StartedAt = DateTime.Now,
                Status = "Active"
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns(call);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new ToggleMute(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            useCase.Execute(callId, userId, isMuted);

            // Assert
            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockParticipantRepo.Verify(
                r => r.SetMute(callId, userId, isMuted),
                Times.Once
            );
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;
            int userId = 88;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new ToggleMute(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, userId, false);

            // Assert
            act.Should()
               .Throw<NotFoundException>()
               .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockParticipantRepo.Verify(
                r => r.SetMute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()),
                Times.Never
            );
        }
    }
}
