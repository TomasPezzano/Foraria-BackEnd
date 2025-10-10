using Foraria.Infrastructure.Blockchain;
using Xunit;
using System.Text;

public class PolygonBlockchainServiceTests
{
    [Fact]
    public void ComputeSha256_ShouldGenerate_ExpectedHash()
    {
        // Arrange
        var text = "Foraria Test Hash";
        var expected = "0x" + BitConverter
            .ToString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(text)))
            .Replace("-", "")
            .ToLowerInvariant();

        // Act
        var hashBytes = PolygonBlockchainService.ComputeSha256(text);
        var hashHex = PolygonBlockchainService.BytesToHex(hashBytes);

        // Assert
        Assert.Equal(expected, hashHex);
    }

    [Fact]
    public async Task ShouldConnectToPolygonAmoy()
    {
        var rpcUrl = "https://rpc-amoy.polygon.technology";
        var web3 = new Nethereum.Web3.Web3(rpcUrl);
        var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

        Assert.True(blockNumber.Value > 0);
    }
}
