using FluentAssertions;
using Foraria.Application.UseCase;
using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ForariaTest.Unit.Blockchain
{
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

            mockBlockchain.Setup(b => b.ComputeSha256FromFile(filePath))
                          .Returns(new byte[] { 0xBB });
            mockBlockchain.Setup(b => b.BytesToHex(It.IsAny<byte[]>()))
                          .Returns(fakeHash);
            mockBlockchain.Setup(b => b.NotarizeAsync(fakeHash, $"app://document/{docId}"))
                          .ReturnsAsync((fakeTx, fakeHash));

            mockRepo.Setup(r => r.AddAsync(It.IsAny<BlockchainProof>()))
                    .ReturnsAsync((BlockchainProof p) => p);

            var useCase = new NotarizeFile(mockBlockchain.Object, mockRepo.Object);

            // Act
            var result = await useCase.ExecuteAsync(docId, filePath);

            // Assert
            result.Should().NotBeNull();
            result.DocumentId.Should().Be(docId);
            result.HashHex.Should().Be(fakeHash);
            result.TxHash.Should().Be(fakeTx);
            result.Uri.Should().Be($"app://document/{docId}");
            result.Network.Should().Be("polygon");
            result.ChainId.Should().Be(80002);

            mockBlockchain.Verify(b => b.NotarizeAsync(fakeHash, $"app://document/{docId}"), Times.Once);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<BlockchainProof>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenFileDoesNotExist()
        {
            var mockBlockchain = new Mock<IBlockchainService>();
            var mockRepo = new Mock<IBlockchainProofRepository>();
            var useCase = new NotarizeFile(mockBlockchain.Object, mockRepo.Object);

            var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            Func<Task> act = async () => await useCase.ExecuteAsync(Guid.NewGuid(), nonExistent);

            await act.Should().ThrowAsync<FileNotFoundException>()
                .WithMessage("No se encontró el archivo a notarizar.*");
        }
    }
}
