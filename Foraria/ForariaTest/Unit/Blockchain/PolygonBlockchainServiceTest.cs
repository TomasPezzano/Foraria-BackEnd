using Foraria.Infrastructure.Blockchain;
using ForariaDomain.Aplication.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using Xunit;

public class PolygonBlockchainServiceTests
{
    private readonly BlockchainService _service;

    public PolygonBlockchainServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Blockchain:RpcUrl", "https://fake-url" },
            { "Blockchain:ContractAddress", "0x0000000000000000000000000000000000000000" },
            { "Blockchain:AbiPath", "Blockchain/ForariaNotary.abi.json" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var settings = Options.Create(new BlockchainSettings
        {
            PrivateKey = "0xabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefabcdefab"
        });

        var abiPath = Path.Combine(AppContext.BaseDirectory, "Blockchain/ForariaNotary.abi.json");
        Directory.CreateDirectory(Path.GetDirectoryName(abiPath)!);
        if (!File.Exists(abiPath))
            File.WriteAllText(abiPath, "{}");

        _service = new BlockchainService(configuration, settings);
    }

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
        var hashBytes = _service.ComputeSha256(text);
        var hashHex = _service.BytesToHex(hashBytes);

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
