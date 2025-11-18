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

        if (string.IsNullOrWhiteSpace(transcript.TranscriptHash))
            throw new DomainValidationException("La transcripción no tiene hash asociado.");

        var existingProof = await _proofRepo.GetByCallTranscriptIdAsync(callTranscriptId);
        if (existingProof != null)
            throw new BlockchainException("Esta transcripción ya está notarizada");

        var hashHex = transcript.TranscriptHash;

        var existing = await _proofRepo.GetByHashHexAsync(hashHex);
        if (existing != null)
            throw new InvalidOperationException(
                "El archivo ya fue notarizado previamente. Si querés volver a registrarlo, modificá el contenido para generar un hash diferente."
            );

        var uri = $"call-transcript:{transcript.CallId}";



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

        transcript.BlockchainTxHash = txHash;

        _proofRepo.Add(proof);
        _transcriptRepo.Update(transcript);

        await _uow.SaveChangesAsync();
    }
}
