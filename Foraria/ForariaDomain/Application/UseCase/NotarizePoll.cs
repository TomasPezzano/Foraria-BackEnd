using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;

namespace Foraria.Application.UseCase
{
    public class NotarizePoll
    {
        private readonly IBlockchainService _blockchain;
        private readonly IBlockchainProofRepository _proofRepo;

        public NotarizePoll(IBlockchainService blockchain, IBlockchainProofRepository proofRepo)
        {
            _blockchain = blockchain;
            _proofRepo = proofRepo;
        }

        public async Task<BlockchainProof> ExecuteAsync(int pollId, string text)
        {
            var uri = $"app://poll/{pollId}";

            var (txHash, hashHex) = await _blockchain.NotarizeAsync(text, uri);

            var proof = new BlockchainProof
            {
                PollId = pollId,
                HashHex = hashHex,
                Uri = uri,
                TxHash = txHash,
                Contract = "0xA8a7BcAb69C858929e03f5f93986F65195aE935C",
                Network = "polygon",
                ChainId = 80002
            };

            await _proofRepo.AddAsync(proof);
            return proof;
        }
    }
}
