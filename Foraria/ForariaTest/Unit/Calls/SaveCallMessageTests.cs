using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class SaveCallMessageTests
    {
        [Fact]
        public void Execute_ShouldSaveMessage_WhenCallExists()
        {
            // Arrange
            int callId = 10;
            int userId = 5;
            string content = "Hola!";

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 1,
                StartedAt = DateTime.UtcNow,
                Status = "Active"
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns(call);

            var mockMessageRepo = new Mock<ICallMessageRepository>();

            var useCase = new SaveCallMessage(mockCallRepo.Object, mockMessageRepo.Object);

            // Act
            useCase.Execute(callId, userId, content);

            // Assert
            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);

            mockMessageRepo.Verify(
                r => r.Save(It.Is<CallMessage>(m =>
                    m.CallId == callId &&
                    m.UserId == userId &&
                    m.Message == content
                )),
                Times.Once
            );
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;
            int userId = 5;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId))
                        .Returns((Call?)null);

            var mockMessageRepo = new Mock<ICallMessageRepository>();

            var useCase = new SaveCallMessage(mockCallRepo.Object, mockMessageRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId, userId, "mensaje");

            // Assert
            act.Should()
               .Throw<NotFoundException>()
               .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockMessageRepo.Verify(r => r.Save(It.IsAny<CallMessage>()), Times.Never);
        }
    }
}
