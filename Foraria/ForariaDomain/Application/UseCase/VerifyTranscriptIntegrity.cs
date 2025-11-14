using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Security.Cryptography;

public class VerifyTranscriptIntegrity
{
    private readonly ICallTranscriptRepository _transcriptRepo;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IConfiguration _config;

    public VerifyTranscriptIntegrity(
        ICallTranscriptRepository transcriptRepo,
        IBlockchainProofRepository proofRepo,
        IConfiguration config)
    {
        _transcriptRepo = transcriptRepo;
        _proofRepo = proofRepo;
        _config = config;
    }

    public async Task<(CallTranscript transcript, BlockchainProof proof, bool hashMatches, bool onChain)>
        ExecuteAsync(int callId)
    {
        var transcript = _transcriptRepo.GetByCallId(callId)
            ?? throw new NotFoundException("No existe transcripción para ese callId.");

        var proof = await _proofRepo.GetByCallTranscriptIdAsync(transcript.Id)
            ?? throw new NotFoundException("No existe prueba blockchain asociada.");

        var baseUrl = _config["TranscriptionService:BaseUrl"]
            ?? throw new InvalidOperationException("TranscriptionService:BaseUrl no configurado.");

        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

        using var remoteStream = await http.GetStreamAsync($"/api/transcriptions/{callId}/transcript-file");

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(remoteStream);
        var localHash = "0x" + BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        bool hashMatches = localHash == proof.HashHex.ToLower();

        bool onChain = proof.TxHash != null;

        return (transcript, proof, hashMatches, onChain);
    }
}
