using Moq;
using Foraria.Application.UseCase;
using Foraria.Infrastructure.Blockchain;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Xunit;

public class NotarizePollTests
{
    [Fact]
    public async Task GivenValidPoll_WhenNotarizing_ShouldSaveProof()
    {
        var mockBlockchain = new Mock<IBlockchainService>();
        mockBlockchain
            .Setup(b => b.NotarizeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(("fakeTxHash", "fakeHashHex"));

        var mockRepo = new Mock<IBlockchainProofRepository>();
        var useCase = new NotarizePoll(mockBlockchain.Object, mockRepo.Object);

        var result = await useCase.ExecuteAsync(1, "texto de prueba");

        Assert.Equal("fakeTxHash", result.TxHash);
        mockBlockchain.Verify(b => b.NotarizeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<BlockchainProof>()), Times.Once);
    }
}