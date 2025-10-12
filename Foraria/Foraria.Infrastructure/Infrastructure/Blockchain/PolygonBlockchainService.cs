using Foraria.Domain.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Foraria.Infrastructure.Blockchain
{
    public class PolygonBlockchainService : IBlockchainService
    {
        private readonly string _rpcUrl;
        private readonly string _privateKey;
        private readonly string _contractAddress;
        private readonly string _abi;
        private readonly Web3 _web3;
        private readonly Account _account;




        public PolygonBlockchainService(IConfiguration configuration, IOptions<BlockchainSettings> settings)
        {
            var blockchainSection = configuration.GetSection("Blockchain");

            _rpcUrl = blockchainSection["RpcUrl"]
                ?? throw new InvalidOperationException("Falta Blockchain:RpcUrl en appsettings.json.");

            _privateKey = settings.Value.PrivateKey
                ?? throw new InvalidOperationException("Private Keysin configurar");

            _contractAddress = blockchainSection["ContractAddress"]
                ?? throw new InvalidOperationException("Falta Blockchain:ContractAddress en appsettings.json.");
            var abiPath = blockchainSection["AbiPath"]
                ?? throw new InvalidOperationException("Falta Blockchain:AbiPath en appsettings.json.");

            if (!Path.IsPathRooted(abiPath))
            {
                abiPath = Path.Combine(AppContext.BaseDirectory, abiPath);
            }

            if (!File.Exists(abiPath))
                throw new FileNotFoundException($"No se encontró el archivo ABI en la ruta: {abiPath}");

            _abi = File.ReadAllText(abiPath);

            _account = new Account(_privateKey);
            _web3 = new Web3(_account, _rpcUrl);
        }

        public byte[] ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public string BytesToHex(byte[] bytes)
            => "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

        public async Task<(string TxHash, string HashHex)> NotarizeAsync(string text, string uri)
        {
            var hashBytes = ComputeSha256(text);
            var hashHex = BytesToHex(hashBytes);

            var account = new Account(_privateKey);
            var web3 = new Web3(account, _rpcUrl);
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var notarize = contract.GetFunction("notarize");

            var gasEstimate = await notarize.EstimateGasAsync(account.Address, null, null, hashBytes, uri);

            var receipt = await notarize.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                new Nethereum.Hex.HexTypes.HexBigInteger(gasEstimate.Value),
                null,
                null,
                hashBytes,
                uri
            );

            return (receipt.TransactionHash, hashHex);
        }

        public byte[] ComputeSha256FromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo especificado.", filePath);

            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return sha.ComputeHash(stream);
        }

        public async Task<bool> VerifyFileAsync(string filePath, string expectedHashHex)
        {
            var hashBytes = ComputeSha256FromFile(filePath);
            var hashHex = BytesToHex(hashBytes);

            if (!string.Equals(hashHex, expectedHashHex, StringComparison.OrdinalIgnoreCase))
                return false;

            var contract = _web3.Eth.GetContract(_abi, _contractAddress);
            var getRecord = contract.GetFunction("getRecord");

            var record = await getRecord.CallDeserializingToObjectAsync<RecordDto>(hashBytes);

            if (record == null || record.Timestamp == 0)
                return false;

            return true;
        }

        public Task<bool> VerifyAsync(string text, string expectedHashHex)
        {
            throw new NotImplementedException();
        }
    }
}
