using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Moq;

namespace ForariaTest.Unit.Calls
{
    public class GetCallDetailsTests
    {
        private readonly Mock<ICallRepository> _mockRepo;
        private readonly GetCallDetails _useCase;

        public GetCallDetailsTests()
        {
            _mockRepo = new Mock<ICallRepository>();
            _useCase = new GetCallDetails(_mockRepo.Object);
        }

        [Fact]
        public void Execute_ShouldReturnCall_WhenCallExists()
        {
            // Arrange
            int callId = 1;
            var call = new Call { Id = callId, Status = "Active" };

            _mockRepo.Setup(r => r.GetById(callId)).Returns(call);

            // Act
            var result = _useCase.Execute(callId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(callId, result.Id);
            _mockRepo.Verify(r => r.GetById(callId), Times.Once);
        }

        [Fact]
        public void Execute_ShouldThrowNotFoundException_WhenCallDoesNotExist()
        {
            // Arrange
            int callId = 999;

            _mockRepo.Setup(r => r.GetById(callId)).Returns((Call?)null);

            // Act
            var act = () => _useCase.Execute(callId);

            // Assert
            Assert.Throws<NotFoundException>(act);
            _mockRepo.Verify(r => r.GetById(callId), Times.Once);
        }
    }
}
