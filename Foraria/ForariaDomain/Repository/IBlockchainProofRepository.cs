using Foraria.Domain.Model;

public interface IBlockchainProofRepository
{
    BlockchainProof Add(BlockchainProof proof);
    Task<BlockchainProof?> GetByPollIdAsync(int pollId);
    Task<BlockchainProof?> GetByDocumentIdAsync(Guid documentId);
    Task<BlockchainProof?> GetByHashAsync(string hashHex);
    Task<BlockchainProof?> GetByCallTranscriptIdAsync(int callTranscriptId);
}