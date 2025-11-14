using FluentAssertions;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Moq;

public class NotarizeFileTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldNotarizeFile_WhenFileExists()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "contenido temporal");

        string fakeHash = "0x" + new string('b', 64);
        string fakeTx = "0x456txhash";

        var mockBlockchain = new Mock<IBlockchainService>();
        var mockRepo = new Mock<IBlockchainProofRepository>();
        var mockUow = new Mock<IUnitOfWork>();

        mockBlockchain.Setup(b => b.ComputeSha256FromFile(filePath))
                      .Returns(new byte[] { 0xBB });

        mockBlockchain.Setup(b => b.BytesToHex(It.IsAny<byte[]>()))
                      .Returns(fakeHash);

        mockBlockchain.Setup(b => b.NotarizeAsync(fakeHash, $"app://document/{docId}"))
                      .ReturnsAsync((fakeTx, fakeHash));

        mockRepo.Setup(r => r.Add(It.IsAny<BlockchainProof>()))
                .Returns((BlockchainProof p) => p);

        mockUow.Setup(u => u.SaveChangesAsync())
               .ReturnsAsync(1);

        var useCase = new NotarizeFile(
            mockBlockchain.Object,
            mockRepo.Object,
            mockUow.Object
        );

        // Act
        var result = await useCase.ExecuteAsync(docId, filePath);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().Be(docId);
        result.HashHex.Should().Be(fakeHash);
        result.TxHash.Should().Be(fakeTx);

        mockRepo.Verify(r => r.Add(It.IsAny<BlockchainProof>()), Times.Once);
        mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        mockBlockchain.Verify(b => b.NotarizeAsync(fakeHash, $"app://document/{docId}"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenFileDoesNotExist()
    {
        var mockBlockchain = new Mock<IBlockchainService>();
        var mockRepo = new Mock<IBlockchainProofRepository>();
        var mockUow = new Mock<IUnitOfWork>();

        var useCase = new NotarizeFile(
            mockBlockchain.Object,
            mockRepo.Object,
            mockUow.Object
        );

        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        Func<Task> act = async () => await useCase.ExecuteAsync(Guid.NewGuid(), nonExistent);

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("No se encontró el archivo a notarizar.*");
    }
}
