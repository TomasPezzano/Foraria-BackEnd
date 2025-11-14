using Foraria.Domain.Model;
using Foraria.Domain.Service;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

public class VerifyTranscriptIntegrity
{
    private readonly ICallTranscriptRepository _transcriptRepo;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IBlockchainService _blockchain;

    public VerifyTranscriptIntegrity(
        ICallTranscriptRepository transcriptRepo,
        IBlockchainProofRepository proofRepo,
        IBlockchainService blockchain)
    {
        _transcriptRepo = transcriptRepo;
        _proofRepo = proofRepo;
        _blockchain = blockchain;
    }

    public async Task<(CallTranscript transcript,
                       BlockchainProof proof,
                       bool hashMatches,
                       bool isValidOnChain)>
        ExecuteAsync(int callId)
    {
        var transcript = _transcriptRepo.GetByCallId(callId)
            ?? throw new NotFoundException("No existe transcripción para ese callId.");

        var proof = await _proofRepo.GetByCallTranscriptIdAsync(transcript.Id)
            ?? throw new NotFoundException("No existe prueba blockchain asociada.");

        var localHash = _blockchain.BytesToHex(
            _blockchain.ComputeSha256FromFile(transcript.TranscriptPath)
        );

        bool hashMatches =
            localHash.Equals(transcript.TranscriptHash, StringComparison.OrdinalIgnoreCase);

        var isValidOnChain =
            await _blockchain.VerifyFileAsync(transcript.TranscriptPath, proof.HashHex);

        return (transcript, proof, hashMatches, isValidOnChain);
    }
}
