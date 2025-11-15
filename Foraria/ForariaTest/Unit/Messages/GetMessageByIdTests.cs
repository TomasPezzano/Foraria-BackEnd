using System.Threading.Tasks;
using Moq;
using Xunit;
using ForariaDomain.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Repository.Foraria.Domain.Repository;
using ForariaDomain;

namespace ForariaTest.Unit.Messages
{
    public class GetMessageByIdTests
    {
        private readonly Mock<IMessageRepository> _repositoryMock;
        private readonly GetMessageById _useCase;

        public GetMessageByIdTests()
        {
            _repositoryMock = new Mock<IMessageRepository>();
            _useCase = new GetMessageById(_repositoryMock.Object);
        }

        [Fact]
        public async Task Execute_ShouldReturnMessage_WhenMessageExists()
        {
            int messageId = 1;
            var expectedMessage = new Message
            {
                Id = messageId,
                Content = "Este es un mensaje de prueba",
                User_id = 1
            };

            _repositoryMock
                .Setup(r => r.GetById(messageId))
                .ReturnsAsync(expectedMessage);

            var result = await _useCase.Execute(messageId);

            Assert.NotNull(result);
            Assert.Equal(expectedMessage.Id, result?.Id);
            Assert.Equal(expectedMessage.Content, result?.Content);
            Assert.Equal(expectedMessage.User_id, result?.User_id);

            _repositoryMock.Verify(r => r.GetById(messageId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnNull_WhenMessageDoesNotExist()
        {
            int messageId = 99;
            _repositoryMock
                .Setup(r => r.GetById(messageId))
                .ReturnsAsync((Message?)null);

            var result = await _useCase.Execute(messageId);

            Assert.Null(result);

            _repositoryMock.Verify(r => r.GetById(messageId), Times.Once);
        }
    }
}
