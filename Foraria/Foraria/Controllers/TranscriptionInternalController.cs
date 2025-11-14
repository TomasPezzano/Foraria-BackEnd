using Foraria.Domain.Repository;
using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[ApiController]
[Route("api/transcriptions")]
public class TranscriptionInternalController : ControllerBase
{
    private readonly RegisterTranscriptionResult _register;
    private readonly FinalizeCallTranscriptionAndNotarize _finalize;
    private readonly IUnitOfWork _uow;
    private readonly ICallTranscriptRepository _transcriptRepo;
    private readonly IBlockchainProofRepository _proofRepo;
    private readonly IConfiguration _config;
    private readonly VerifyTranscriptIntegrity _verifyTranscriptIntegrity;

    public TranscriptionInternalController(
        RegisterTranscriptionResult register,
        FinalizeCallTranscriptionAndNotarize finalize,
        IUnitOfWork uow,
        ICallTranscriptRepository transcriptRepo,
        IBlockchainProofRepository proofRepo,
        IConfiguration config,
        VerifyTranscriptIntegrity verifyTranscriptIntegrity)
    {
        _register = register;
        _finalize = finalize;
        _uow = uow;
        _transcriptRepo = transcriptRepo;
        _proofRepo = proofRepo;
        _config = config;
        _verifyTranscriptIntegrity = verifyTranscriptIntegrity;
    }

    [HttpPost("internal/{callId}/complete")]
    [SwaggerOperation(
        Summary = "Registra el resultado de la transcripción.",
        Description = "Endpoint llamado por el microservicio de transcripción."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous] // microservicio
    public async Task<IActionResult> Complete(int callId, [FromBody] CallTranscriptCompleteDto request)
    {
        var existingByHash = await _proofRepo.GetByHashHexAsync(request.TranscriptHash);
        if (existingByHash != null)
            throw new InvalidOperationException(
                "El archivo ya fue notarizado previamente. No se puede registrar nuevamente."
            );

        var transcript = _register.Execute(
            callId,
            request.TranscriptPath,
            request.AudioPath,
            request.TranscriptHash,
            request.AudioHash
        );

        await _uow.SaveChangesAsync();

        await _finalize.ExecuteAsync(transcript.Id);

        return Ok(new { message = "Transcripción registrada y notarizada con éxito." });
    }


    [HttpGet("{callId}/info")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene información de la transcripción y enlaces relacionados.",
        Description = "Devuelve hashes, transacción en blockchain y URLs para descargar audio / transcript."
    )]
    public async Task<IActionResult> GetTranscriptInfo(int callId)
    {
        var transcript = _transcriptRepo.GetByCallId(callId);
        if (transcript == null)
            return NotFound(new { message = "No existe transcripción para ese callId." });

        var proof = await _proofRepo.GetByCallTranscriptIdAsync(transcript.Id);

        var baseUrl = _config["TranscriptionService:BaseUrl"]
            ?? throw new InvalidOperationException("TranscriptionService:BaseUrl no configurado.");

        var transcriptUrl = $"{baseUrl}/api/transcriptions/{callId}/transcript-file";
        var audioUrl = $"{baseUrl}/api/transcriptions/{callId}/audio-file";

        var explorerUrl = proof?.TxHash != null
            ? $"https://amoy.polygonscan.com/tx/{proof.TxHash}"
            : null;

        return Ok(new
        {
            callId,
            transcriptId = transcript.Id,
            transcript.TranscriptHash,
            transcript.AudioHash,
            transcript.BlockchainTxHash,
            transcript.CreatedAt,
            blockchainProofId = proof?.Id,
            proof?.HashHex,
            proof?.TxHash,
            proof?.Uri,
            explorerUrl,
            download = new
            {
                transcriptUrl,
                audioUrl
            }
        });
    }


    [HttpGet("{callId}/verify")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Verifica integridad de la transcripción y su validez en blockchain.",
        Description = "Recalcula hash, compara con DB y verifica contra la prueba en blockchain."
    )]
    public async Task<IActionResult> Verify(int callId)
    {
        var (transcript, proof, hashMatches, isValidOnChain) =
            await _verifyTranscriptIntegrity.ExecuteAsync(callId);

        var explorerUrl = proof.TxHash != null
            ? $"https://amoy.polygonscan.com/tx/{proof.TxHash}"
            : null;

        return Ok(new
        {
            callId,
            transcriptId = transcript.Id,
            transcript.TranscriptHash,
            transcript.AudioHash,
            proofId = proof.Id,
            proof.HashHex,
            proof.TxHash,
            matchesLocalFile = hashMatches,
            validOnBlockchain = isValidOnChain,
            explorerUrl
        });
    }
}
