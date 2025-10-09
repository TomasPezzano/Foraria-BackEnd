using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Security.Cryptography;
using System.Text;

namespace Foraria.Infrastructure.Blockchain
{
    public class PolygonBlockchainService
    {
        private readonly string _rpcUrl;
        private readonly string _privateKey;
        private readonly string _contractAddress;
        private readonly string _abi;
        private readonly Web3 _web3;
        private readonly Account _account;

        public PolygonBlockchainService(string rpcUrl, string privateKey, string contractAddress, string abi)
        {
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentException("La clave privada no está configurada.");

            _rpcUrl = rpcUrl;
            _privateKey = privateKey;
            _contractAddress = contractAddress;
            _abi = abi;

            _account = new Account(_privateKey);
            _web3 = new Web3(_account, _rpcUrl);
        }

        public static byte[] ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string BytesToHex(byte[] bytes)
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

    }
}
