using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Security.Cryptography;
using System.Text;
using Foraria.Domain.Repository;

namespace Foraria.Infrastructure.Blockchain
{
    public class PolygonBlockchainService : IBlockchainService
    {
        private readonly string _rpcUrl;
        private readonly string _privateKey;
        private readonly string _contractAddress;
        private readonly string _abi;

        public PolygonBlockchainService(string rpcUrl, string privateKey, string contractAddress, string abi)
        {
            _rpcUrl = rpcUrl;
            _privateKey = privateKey;
            _contractAddress = contractAddress;
            _abi = abi;
        }

        public static byte[] ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string BytesToHex(byte[] bytes)
            => "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

        public async Task<(string txHash, string hashHex)> NotarizeAsync(string text, string uri)
        {
            var hashBytes = ComputeSha256(text);
            var hashHex = BytesToHex(hashBytes);

            var account = new Account(_privateKey);
            var web3 = new Web3(account, _rpcUrl);
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var notarize = contract.GetFunction("notarize");

            var receipt = await notarize
                .SendTransactionAndWaitForReceiptAsync(account.Address, null, null, null, hashHex, uri);

            return (receipt.TransactionHash, hashHex);
        }
    }
}
