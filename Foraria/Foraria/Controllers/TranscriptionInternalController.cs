using Foraria.Domain.Repository;
using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[ApiController]
[Route("internal/transcription")]
public class TranscriptionInternalController : ControllerBase
{
    private readonly RegisterTranscriptionResult _register;
    private readonly FinalizeCallTranscriptionAndNotarize _finalize;
    private readonly IUnitOfWork _uow;

    public TranscriptionInternalController(
        RegisterTranscriptionResult register,
        FinalizeCallTranscriptionAndNotarize finalize,
        IUnitOfWork uow)
    {
        _register = register;
        _finalize = finalize;
        _uow = uow;
    }

    [HttpPost("{callId}/complete")]
    [SwaggerOperation(
        Summary = "Registra el resultado de la transcripción.",
        Description = "Este endpoint es llamado por el microservicio de transcripción. Registra los archivos y notariza la información."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(int callId, [FromBody] CallTranscriptCompleteDto request)
    {
        var transcript = _register.Execute(callId, request.TranscriptPath, request.AudioPath);

        await _uow.SaveChangesAsync();

        await _finalize.ExecuteAsync(transcript.Id);

        return Ok(new { message = "Transcripción registrada y notarizada con éxito." });
    }
}
