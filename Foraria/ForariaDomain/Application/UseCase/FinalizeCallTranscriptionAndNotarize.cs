using Foraria.Domain.Model;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

public class FinalizeCallTranscriptionAndNotarize
{
    private readonly ICallTranscriptRepository _transcriptRepo;
    private readonly IBlockchainService _blockchain;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IUnitOfWork _uow;

    public FinalizeCallTranscriptionAndNotarize(
        ICallTranscriptRepository transcriptRepo,
        IBlockchainService blockchain,
        IBlockchainProofRepository proofRepo,
        IUnitOfWork uow)
    {
        _transcriptRepo = transcriptRepo;
        _blockchain = blockchain;
        _proofRepo = proofRepo;
        _uow = uow;
    }

    public async Task ExecuteAsync(int callTranscriptId)
    {
        var transcript = _transcriptRepo.GetById(callTranscriptId)
            ?? throw new NotFoundException("Transcripción no encontrada.");


        var hashBytes = _blockchain.ComputeSha256FromFile(transcript.TranscriptPath);
        var hashHex = _blockchain.BytesToHex(hashBytes);


        var uri = $"file://{transcript.TranscriptPath}";


        var (txHash, finalHashHex) = await _blockchain.NotarizeAsync(hashHex, uri);

        var proof = new BlockchainProof
        {
            CallTranscriptId = callTranscriptId,
            HashHex = finalHashHex,
            Uri = uri,
            TxHash = txHash,
            Contract = _blockchain.ContractAddress, 
            Network = "polygon",
            ChainId = 80002
        };

        _proofRepo.Add(proof);
        await _uow.SaveChangesAsync();
    }
}
