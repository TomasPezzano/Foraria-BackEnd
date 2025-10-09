using Foraria.Domain.Model;

namespace Foraria.Domain.Repository
{
    public interface IBlockchainProofRepository
    {
        Task<BlockchainProof> AddAsync(BlockchainProof proof);
        Task<BlockchainProof?> GetByPollIdAsync(int pollId);
    }
}