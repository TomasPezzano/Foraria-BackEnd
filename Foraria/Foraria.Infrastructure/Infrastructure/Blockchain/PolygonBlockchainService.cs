using Foraria.Domain.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.JsonRpc.Client;
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
                ?? throw new InvalidOperationException("Private Key sin configurar");

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


        public async Task<(string TxHash, string HashHex)> NotarizeAsync(string hashHex, string uri)
        {
            if (string.IsNullOrWhiteSpace(hashHex))
                throw new ArgumentException("hashHex vacío", nameof(hashHex));

            if (!hashHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hashHex = "0x" + hashHex;
            if (hashHex.Length != 66)
                throw new ArgumentException("hashHex debe ser un SHA-256 de 32 bytes (64 hex).", nameof(hashHex));

            await EnsureContractReadyAsync();

            var hashBytes32 = hashHex.HexToByteArray();

            var contract = _web3.Eth.GetContract(_abi, _contractAddress);
            var isNotarizedFn = contract.GetFunction("isNotarized");
            var already = await isNotarizedFn.CallAsync<bool>(hashBytes32);
            if (already)
                throw new InvalidOperationException("Already notarized");

            var notarize = contract.GetFunction("notarize");

            Nethereum.Hex.HexTypes.HexBigInteger gas;
            try
            {
                var est = await notarize.EstimateGasAsync(_account.Address, null, null, hashBytes32, uri);
                gas = new Nethereum.Hex.HexTypes.HexBigInteger(est.Value);
            }
            catch
            {
                gas = new Nethereum.Hex.HexTypes.HexBigInteger(300_000);
            }

            try
            {
                var receipt = await notarize.SendTransactionAndWaitForReceiptAsync(
                    _account.Address,
                    gas,
                    null,
                    null,
                    hashBytes32,
                    uri
                );

                return (receipt.TransactionHash, hashHex.ToLowerInvariant());
            }
            catch (RpcResponseException ex)
            {
                throw new InvalidOperationException($"Smart contract error: {ex.RpcError?.Message ?? ex.Message}", ex);
            }
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
            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo especificado.", filePath);

            var hashBytes = ComputeSha256FromFile(filePath);
            var hashHex = BytesToHex(hashBytes);

            if (!string.Equals(hashHex, expectedHashHex, StringComparison.OrdinalIgnoreCase))
                return false;

            var contract = _web3.Eth.GetContract(_abi, _contractAddress);
            var getRecord = contract.GetFunction("getRecord");

            if (!hashHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hashHex = "0x" + hashHex;

            var hashBytes32 = hashHex.HexToByteArray();

            var record = await getRecord.CallDeserializingToObjectAsync<RecordDto>(hashBytes32);

            if (record == null || record.Timestamp == 0)
                return false;

            return true;
        }


        public Task<bool> VerifyAsync(string text, string expectedHashHex)
        {
            throw new NotImplementedException();
        }
        private async Task EnsureContractReadyAsync()
        {
            var code = await _web3.Eth.GetCode.SendRequestAsync(_contractAddress);
            if (string.Equals(code, "0x", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"No hay contrato en la dirección configurada: '{_contractAddress}'.");


        }
    }

}
