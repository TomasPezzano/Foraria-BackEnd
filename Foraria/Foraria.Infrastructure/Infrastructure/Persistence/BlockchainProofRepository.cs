using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Repository
{
    public class BlockchainProofRepository : IBlockchainProofRepository
    {
        private readonly ForariaContext _context;

        public BlockchainProofRepository(ForariaContext context)
        {
            _context = context;
        }

        public async Task<BlockchainProof> AddAsync(BlockchainProof proof)
        {
            _context.BlockchainProofs.Add(proof);
            await _context.SaveChangesAsync();
            return proof;
        }

        public async Task<BlockchainProof?> GetByPollIdAsync(int pollId)
        {
            return await _context.BlockchainProofs
                .FirstOrDefaultAsync(p => p.PollId == pollId);
        }
    }
}
