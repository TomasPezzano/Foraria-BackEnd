using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Foraria.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class JoinCallTests
    {
        [Fact]
        public void Execute_ShouldAddParticipant_WhenCallExistsAndIsActive()
        {
            // Arrange
            int callId = 5;
            int userId = 99;

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

            var useCase = new JoinCall(mockCallRepo.Object, mockParticipantRepo.Object);

            // Act
            useCase.Execute(callId, userId);

            // Assert
            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockParticipantRepo.Verify(
                r => r.Add(It.Is<CallParticipant>(p =>
                    p.CallId == callId &&
                    p.UserId == userId
                )),
                Times.Once
            );
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 555;
            int userId = 22;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new JoinCall(mockCallRepo.Object, mockParticipantRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, userId);

            // Assert
            act.Should()
               .Throw<NotFoundException>()
               .WithMessage("Llamada no encontrada");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockParticipantRepo.Verify(r => r.Add(It.IsAny<CallParticipant>()), Times.Never);
        }

        [Fact]
        public void Execute_ShouldThrowInvalidOperation_WhenCallIsNotActive()
        {
            // Arrange
            int callId = 10;
            int userId = 22;

            var call = new Call
            {
                Id = callId,
                Status = "Ended", // cualquier valor != "Active"
                CreatedByUserId = 1,
                StartedAt = DateTime.Now
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns(call);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new JoinCall(mockCallRepo.Object, mockParticipantRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, userId);

            // Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("No te puedes unir a una llamada que no esta activa.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockParticipantRepo.Verify(r => r.Add(It.IsAny<CallParticipant>()), Times.Never);
        }
    }
}
