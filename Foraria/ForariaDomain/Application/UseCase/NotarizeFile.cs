using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;

namespace Foraria.Application.UseCase
{
    public class NotarizeFile
    {
        private readonly IBlockchainService _blockchain;
        private readonly IBlockchainProofRepository _proofRepo;

        public NotarizeFile(IBlockchainService blockchain, IBlockchainProofRepository proofRepo)
        {
            _blockchain = blockchain;
            _proofRepo = proofRepo;
        }

        public async Task<BlockchainProof> ExecuteAsync(Guid documentId, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("No se encontró el archivo a notarizar.", filePath);

            var hashBytes = _blockchain.ComputeSha256FromFile(filePath);
            var hashHex = _blockchain.BytesToHex(hashBytes);

            var uri = $"app://document/{documentId}";
            var (txHash, _) = await _blockchain.NotarizeAsync(hashHex, uri);

            var proof = new BlockchainProof
            {
                DocumentId = documentId,
                HashHex = hashHex,
                Uri = uri,
                TxHash = txHash,
                Contract = "0xA8a7BcAb69C858929e03f5f93986F65195aE935C",
                Network = "polygon",
                ChainId = 80002,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _proofRepo.AddAsync(proof);
            return proof;
        }
    }
}
