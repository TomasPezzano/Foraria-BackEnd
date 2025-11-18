using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/calls")]
    public class CallController : ControllerBase
    {
        private readonly CreateCall _createCall;
        private readonly JoinCall _joinCall;
        private readonly EndCall _endCall;
        private readonly GetCallDetails _getCallDetails;
        private readonly GetCallParticipants _getCallParticipants;
        private readonly GetCallMessages _getCallMessages;
        private readonly SaveCallRecording _saveCallRecording;
        private readonly SendCallMessage _sendCallMessage;

        public CallController(
            CreateCall createCall,
            JoinCall joinCall,
            EndCall endCall,
            GetCallDetails getCallDetails,
            GetCallParticipants getCallParticipants,
            GetCallMessages getCallMessages,
            SaveCallRecording saveCallRecording,
            SendCallMessage sendCallMessage)
        {
            _createCall = createCall;
            _joinCall = joinCall;
            _endCall = endCall;
            _getCallDetails = getCallDetails;
            _getCallParticipants = getCallParticipants;
            _getCallMessages = getCallMessages;
            _saveCallRecording = saveCallRecording;
            _sendCallMessage = sendCallMessage;
        }

        [HttpPost]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Crea una videollamada.")]
        [ProducesResponseType(typeof(CallDto), StatusCodes.Status200OK)]
        public IActionResult Create([FromBody] CallCreateDto request)
        {
            if (request == null || request.UserId <= 0)
                throw new DomainValidationException("Datos inválidos.");

            var call = _createCall.Execute(request.UserId);

            return Ok(new CallDto
            {
                Id = call.Id,
                CreatedByUserId = call.CreatedByUserId,
                StartedAt = call.StartedAt,
                Status = call.Status
            });
        }


        [HttpPost("{callId}/join")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Unirse a una videollamada.")]
        public IActionResult Join(int callId, [FromBody] CallJoinDto request)
        {
            if (callId <= 0 || request.UserId <= 0)
                throw new DomainValidationException("Datos inválidos.");

            _joinCall.Execute(callId, request.UserId);

            return Ok(new { message = "Usuario unido a la llamada." });
        }


        [HttpPost("{callId}/end")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Finaliza una videollamada.")]
        public IActionResult End(int callId)
        {
            if (callId <= 0)
                throw new DomainValidationException("ID inválido.");

            _endCall.Execute(callId);

            return Ok(new { message = "Llamada finalizada." });
        }


        [HttpGet("{callId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Obtiene datos de una llamada.")]
        public IActionResult GetCall(int callId)
        {
            if (callId <= 0)
                throw new DomainValidationException("ID inválido.");

            return Ok(_getCallDetails.Execute(callId));
        }


        [HttpGet("{callId}/participants")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Obtiene los participantes.")]
        public IActionResult GetParticipants(int callId)
        {
            return Ok(_getCallParticipants.Execute(callId));
        }


        [HttpGet("{callId}/state")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Estado completo de la llamada.")]
        public IActionResult GetCallState(int callId)
        {
            return Ok(new
            {
                participants = _getCallParticipants.Execute(callId),
                messages = _getCallMessages.Execute(callId)
            });
        }

        [HttpPost("{callId}/recording")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Sube una grabación de la llamada.")]
        public async Task<IActionResult> UploadRecording(int callId, IFormFile file)
        {
            if (callId <= 0)
                throw new DomainValidationException("ID inválido.");

            if (file == null || file.Length == 0)
                throw new DomainValidationException("Archivo inválido.");

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");

            using (var stream = System.IO.File.Create(tempPath))
                await file.CopyToAsync(stream);

            _saveCallRecording.Execute(callId, tempPath, file.ContentType);

            return Ok(new { message = "Grabación subida correctamente." });
        }

        [HttpPost("{callId}/messages")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Envía un mensaje dentro de la llamada.")]
        [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
        public IActionResult SendMessage(
            int callId,
            [FromBody] SendCallMessageDto request)
        {
            if (callId <= 0 || request.UserId <= 0)
                throw new DomainValidationException("Datos inválidos.");

            var result = _sendCallMessage.Execute(callId, request.UserId, request.Message);

            return Ok(new ChatMessageDto
            {
                UserId = result.UserId,
                Message = result.Message,
                SentAt = result.SentAt
            });
        }


    }
}
