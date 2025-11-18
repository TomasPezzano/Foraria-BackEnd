using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaDomain.Repository.ForariaDomain.Repository;
using ForariaDomain.Application.UseCase.Foraria.Application.UseCase;

namespace ForariaTest.Unit.CallTests
{
    public class GetCallRecordingsTests
    {
        [Fact]
        public void Execute_ShouldReturnRecordings_WhenCallExists()
        {
            // Arrange
            int callId = 33;

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 10,
                StartedAt = DateTime.Now
            };

            var recordings = new List<CallRecording>
            {
                new CallRecording { Id = 1, CallId = callId, FilePath = "file1.mp4", ContentType = "video/mp4" },
                new CallRecording { Id = 2, CallId = callId, FilePath = "file2.mp4", ContentType = "video/mp4" }
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns(call);

            var mockRecordingRepo = new Mock<ICallRecordingRepository>();
            mockRecordingRepo.Setup(r => r.GetByCall(callId))
                             .Returns(recordings);

            var useCase = new GetCallRecordings(mockCallRepo.Object, mockRecordingRepo.Object);

            // Act
            var result = useCase.Execute(callId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(recordings);

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockRecordingRepo.Verify(r => r.GetByCall(callId), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockRecordingRepo = new Mock<ICallRecordingRepository>();

            var useCase = new GetCallRecordings(mockCallRepo.Object, mockRecordingRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId);

            // Assert
            act.Should()
                .Throw<NotFoundException>()
                .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockRecordingRepo.Verify(r => r.GetByCall(It.IsAny<int>()), Times.Never);
        }
    }
}
