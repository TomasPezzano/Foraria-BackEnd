using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Moq;

namespace ForariaTest.Unit.Messages
{
    public class HideMessageTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldHideMessage_WhenMessageExistsAndNotHidden()
        {
            int messageId = 1;
            var message = new Message { Id = messageId, Content = "Test", State = "Visible" };

            var repositoryMock = new Mock<IMessageRepository>();
            repositoryMock.Setup(r => r.GetById(messageId)).ReturnsAsync(message);
            repositoryMock.Setup(r => r.Update(It.IsAny<Message>())).Returns(Task.CompletedTask);

            var hideMessage = new HideMessage(repositoryMock.Object);

            var result = await hideMessage.ExecuteAsync(messageId);

            Assert.True(result);
            Assert.Equal("Hidden", message.State);
            repositoryMock.Verify(r => r.Update(message), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenMessageDoesNotExist()
        {
            int messageId = 99;

            var repositoryMock = new Mock<IMessageRepository>();
            repositoryMock.Setup(r => r.GetById(messageId)).ReturnsAsync((Message)null);

            var hideMessage = new HideMessage(repositoryMock.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => hideMessage.ExecuteAsync(messageId));
            Assert.Equal($"No se encontró el mensaje con ID {messageId}", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenMessageAlreadyHidden()
        {
            int messageId = 2;
            var message = new Message { Id = messageId, Content = "Test", State = "Hidden" };

            var repositoryMock = new Mock<IMessageRepository>();
            repositoryMock.Setup(r => r.GetById(messageId)).ReturnsAsync(message);

            var hideMessage = new HideMessage(repositoryMock.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => hideMessage.ExecuteAsync(messageId));
            Assert.Equal("El mensaje ya se encuentra oculto.", ex.Message);
        }
    }
}
