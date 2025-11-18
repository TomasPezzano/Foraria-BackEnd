using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Foraria.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class GetCallParticipantsTests
    {
        [Fact]
        public void Execute_ShouldReturnParticipants_WhenCallExists()
        {
            // Arrange
            int callId = 20;

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 5,
                StartedAt = DateTime.Now,
                Status = "Active"
            };

            var participants = new List<CallParticipant>
            {
                new CallParticipant { Id = 1, CallId = callId, UserId = 2 },
                new CallParticipant { Id = 2, CallId = callId, UserId = 3 }
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId)).Returns(call);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();
            mockParticipantRepo.Setup(r => r.GetParticipants(callId))
                               .Returns(participants);

            var useCase = new GetCallParticipants(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            var result = useCase.Execute(callId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(participants);

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockParticipantRepo.Verify(r => r.GetParticipants(callId), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId)).Returns((Call?)null);

            var mockParticipantRepo = new Mock<ICallParticipantRepository>();

            var useCase = new GetCallParticipants(mockParticipantRepo.Object, mockCallRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId);

            // Assert
            act.Should()
                .Throw<NotFoundException>()
                .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockParticipantRepo.Verify(r => r.GetParticipants(It.IsAny<int>()), Times.Never);
        }
    }
}
