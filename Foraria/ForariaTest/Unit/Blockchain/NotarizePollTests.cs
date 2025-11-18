using FluentAssertions;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Moq;

public class NotarizePollTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldNotarizePoll_WhenValidText()
    {
        // Arrange
        int pollId = 1;
        string text = "Contenido de la votación";
        string fakeHash = "0x" + new string('a', 64);
        string fakeTx = "0x123txhash";

        var mockBlockchain = new Mock<IBlockchainService>();
        var mockRepo = new Mock<IBlockchainProofRepository>();
        var mockUow = new Mock<IUnitOfWork>();

        mockBlockchain.Setup(b => b.ComputeSha256(It.IsAny<string>()))
                      .Returns(new byte[] { 0xAA, 0xBB });

        mockBlockchain.Setup(b => b.BytesToHex(It.IsAny<byte[]>()))
                      .Returns(fakeHash);

        mockBlockchain.Setup(b => b.NotarizeAsync(fakeHash, $"app://poll/{pollId}"))
                      .ReturnsAsync((fakeTx, fakeHash));

        mockRepo.Setup(r => r.Add(It.IsAny<BlockchainProof>()))
                .Returns((BlockchainProof p) => p);

        mockUow.Setup(u => u.SaveChangesAsync())
               .ReturnsAsync(1);

        var useCase = new NotarizePoll(
            mockBlockchain.Object,
            mockRepo.Object,
            mockUow.Object
        );

        // Act
        var result = await useCase.ExecuteAsync(pollId, text);

        // Assert
        result.Should().NotBeNull();
        result.PollId.Should().Be(pollId);
        result.HashHex.Should().Be(fakeHash);
        result.TxHash.Should().Be(fakeTx);

        mockRepo.Verify(r => r.Add(It.IsAny<BlockchainProof>()), Times.Once);
        mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        mockBlockchain.Verify(b => b.NotarizeAsync(fakeHash, $"app://poll/{pollId}"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTextIsEmpty()
    {
        var mockBlockchain = new Mock<IBlockchainService>();
        var mockRepo = new Mock<IBlockchainProofRepository>();
        var mockUow = new Mock<IUnitOfWork>();

        var useCase = new NotarizePoll(
            mockBlockchain.Object,
            mockRepo.Object,
            mockUow.Object
        );

        Func<Task> act = async () => await useCase.ExecuteAsync(1, "  ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El texto de la votación no puede estar vacío.*");
    }
}
