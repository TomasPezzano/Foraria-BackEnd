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

        public BlockchainProof Add(BlockchainProof proof)
        {
            _context.BlockchainProofs.Add(proof);
            return proof;
        }

        public async Task<BlockchainProof?> GetByPollIdAsync(int pollId)
        {
            return await _context.BlockchainProofs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PollId == pollId);
        }

        public async Task<BlockchainProof?> GetByDocumentIdAsync(Guid documentId)
        {
            return await _context.BlockchainProofs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.DocumentId == documentId);
        }

        public async Task<BlockchainProof?> GetByHashAsync(string hashHex)
        {
            return await _context.BlockchainProofs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.HashHex == hashHex);
        }

        public async Task<BlockchainProof?> GetByCallTranscriptIdAsync(int callTranscriptId)
        {
            return await _context.BlockchainProofs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CallTranscriptId == callTranscriptId);
        }
        public Task<BlockchainProof?> GetByHashHexAsync(string hashHex)
            => _context.BlockchainProofs
                .FirstOrDefaultAsync(x => x.HashHex.ToLower() == hashHex.ToLower());
    }
}
