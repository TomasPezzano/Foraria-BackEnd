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
        // Arrange
        var mockBlockchain = new Mock<PolygonBlockchainService>(null, null, null, null);
        mockBlockchain.Setup(b => b.NotarizeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(("0xtx123", "0xhash456"));

        var mockRepo = new Mock<IBlockchainProofRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<BlockchainProof>()))
            .ReturnsAsync((BlockchainProof p) => p);

        var useCase = new NotarizePoll(mockBlockchain.Object, mockRepo.Object);

        // Act
        var proof = await useCase.ExecuteAsync(1, "Texto de prueba");

        // Assert
        Assert.Equal("0xtx123", proof.TxHash);
        Assert.Equal("0xhash456", proof.HashHex);
        Assert.Equal(1, proof.PollId);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<BlockchainProof>()), Times.Once);
    }
}