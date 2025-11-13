using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nethereum.RPC.Eth.Transactions;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Foraria.Controllers;

[ApiController]
[Route("api/calls")]
public class CallController : ControllerBase
{
    private readonly CreateCall _createCall;
    private readonly JoinCall _joinCall;
    private readonly EndCall _endCall;

    public CallController(
        CreateCall createCall,
        JoinCall joinCall,
        EndCall endCall)
    {
        _createCall = createCall;
        _joinCall = joinCall;
        _endCall = endCall;
    }

    [HttpPost]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Crea una nueva videollamada.",
        Description = "Inicia una nueva videollamada asociada al usuario actual."
    )]
    [ProducesResponseType(typeof(CallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CallCreateDto request)
    {
        if (request == null)
            throw new DomainValidationException("El cuerpo de la solicitud no puede estar vacío.");

        var call = _createCall.Execute(request.UserId);

        var dto = new CallDto
        {
            Id = call.Id,
            CreatedByUserId = call.CreatedByUserId,
            StartedAt = call.StartedAt,
            Status = call.Status
        };

        return Ok(dto);
    }

    [HttpPost("{callId}/join")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Une a un usuario a una videollamada.",
        Description = "Registra el ingreso del usuario como participante en la videollamada."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Join(int callId, [FromBody] CallJoinDto request)
    {
        if (callId <= 0)
            throw new DomainValidationException("El ID de la llamada es inválido.");

        _joinCall.Execute(callId, request.UserId);

        return Ok(new { message = "Usuario unido a la llamada." });
    }

    [HttpPost("{callId}/end")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Finaliza una videollamada.",
        Description = "Marca la videollamada como finalizada."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult End(int callId)
    {
        if (callId <= 0)
            throw new DomainValidationException("El ID de la llamada es inválido.");

        _endCall.Execute(callId);

        return Ok(new { message = "Llamada finalizada correctamente." });
    }
}
