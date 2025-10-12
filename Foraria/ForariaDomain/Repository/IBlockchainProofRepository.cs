using Foraria.Domain.Model;

namespace Foraria.Domain.Repository
{
    public interface IBlockchainProofRepository
    {
        Task<BlockchainProof> AddAsync(BlockchainProof proof);
        Task<BlockchainProof?> GetByPollIdAsync(int pollId);
        Task<BlockchainProof?> GetByDocumentIdAsync(Guid documentId);
        Task<BlockchainProof?> GetByHashAsync(string hashHex);
    }
}