using FluentAssertions;
using Moq;
using Xunit;
using ForariaDomain;
using ForariaDomain.Exceptions;
using Foraria.Application.UseCase;
using ForariaDomain.Repository;

namespace ForariaTest.Unit.CallTests
{
    public class GetCallMessagesTests
    {
        [Fact]
        public void Execute_ShouldReturnMessages_WhenCallExists()
        {
            // Arrange
            int callId = 10;

            var call = new Call
            {
                Id = callId,
                CreatedByUserId = 1,
                StartedAt = DateTime.UtcNow
            };

            var messages = new List<CallMessage>
            {
                new CallMessage { Id = 1, CallId = callId, UserId = 2, Message = "Hola!" },
                new CallMessage { Id = 2, CallId = callId, UserId = 3, Message = "¿Todo bien?" }
            };

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId)).Returns(call);

            var mockMessageRepo = new Mock<ICallMessageRepository>();
            mockMessageRepo.Setup(r => r.GetLastByCall(callId, It.IsAny<int>()))
                           .Returns(messages);

            var useCase = new GetCallMessages(mockCallRepo.Object, mockMessageRepo.Object);

            // Act
            var result = useCase.Execute(callId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(messages);

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockMessageRepo.Verify(r => r.GetLastByCall(callId, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFound_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;

            var mockCallRepo = new Mock<ICallRepository>();
            mockCallRepo.Setup(r => r.GetById(callId)).Returns((Call?)null);

            var mockMessageRepo = new Mock<ICallMessageRepository>();

            var useCase = new GetCallMessages(mockCallRepo.Object, mockMessageRepo.Object);

            // Act
            Action act = () => useCase.Execute(callId);

            // Assert
            act.Should()
                .Throw<NotFoundException>()
                .WithMessage("La llamada no existe.");

            mockCallRepo.Verify(r => r.GetById(callId), Times.Once);
            mockMessageRepo.Verify(r => r.GetLastByCall(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
