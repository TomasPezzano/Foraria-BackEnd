using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using ForariaDomain.Repository.ForariaDomain.Repository;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit.CallTests
{
    public class SaveCallRecordingTests
    {
        [Fact]
        public void Execute_ShouldSaveRecording_WhenCallExists()
        {
            // Arrange
            int callId = 7;
            string filePath = "recordings/call7.mp4";
            string contentType = "video/mp4";

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

            var mockRecordingRepo = new Mock<ICallRecordingRepository>();

            var useCase = new SaveCallRecording(mockCallRepo.Object, mockRecordingRepo.Object);

            // Act
            useCase.Execute(callId, filePath, contentType);

            // Assert
            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockRecordingRepo.Verify(
                r => r.Save(It.Is<CallRecording>(rec =>
                    rec.CallId == callId &&
                    rec.FilePath == filePath &&
                    rec.ContentType == contentType
                )),
                Times.Once
            );
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 555;
            string filePath = "whatever.mp4";
            string contentType = "video/mp4";

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockRecordingRepo = new Mock<ICallRecordingRepository>();

            var useCase = new SaveCallRecording(mockCallRepo.Object, mockRecordingRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, filePath, contentType);

            // Assert
            act.Should()
               .Throw<NotFoundException>()
               .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockRecordingRepo.Verify(r => r.Save(It.IsAny<CallRecording>()), Times.Never);
        }
    }
}
