using Foraria.Application.UseCase;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Foraria.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/calls")]
    public class CallController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        private readonly CreateCall _createCall;
        private readonly JoinCall _joinCall;
        private readonly EndCall _endCall;
        private readonly GetCallDetails _getCallDetails;
        private readonly GetCallParticipants _getCallParticipants;
        private readonly GetCallMessages _getCallMessages;
        private readonly SaveCallRecording _saveCallRecording;
        private readonly SendCallMessage _sendCallMessage;
        private readonly GetCallsByConsortium _getCallsByConsortium;

        public CallController(
            IPermissionService permissionService,
            CreateCall createCall,
            JoinCall joinCall,
            EndCall endCall,
            GetCallDetails getCallDetails,
            GetCallParticipants getCallParticipants,
            GetCallMessages getCallMessages,
            SaveCallRecording saveCallRecording,
            SendCallMessage sendCallMessage,
            GetCallsByConsortium getCallsByConsortium)
        {
            _permissionService = permissionService;

            _createCall = createCall;
            _joinCall = joinCall;
            _endCall = endCall;
            _getCallDetails = getCallDetails;
            _getCallParticipants = getCallParticipants;
            _getCallMessages = getCallMessages;
            _saveCallRecording = saveCallRecording;
            _sendCallMessage = sendCallMessage;
            _getCallsByConsortium = getCallsByConsortium;
        }

        [HttpPost]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Crea una videollamada o reunión.")]
        [ProducesResponseType(typeof(CallDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CallCreateDto request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.Create");

            if (request == null || request.UserId <= 0)
                throw new DomainValidationException("Datos inválidos.");

            var call = new Call
            {
                CreatedByUserId = request.UserId,
                Title = request.Title,
                Description = request.Description,
                MeetingType = request.MeetingType,
                ConsortiumId = request.ConsortiumId,
            };

            var created = _createCall.Execute(call);

            return Ok(new CallDto
            {
                Id = created.Id,
                CreatedByUserId = created.CreatedByUserId,
                Title = created.Title,
                Description = created.Description,
                MeetingType = created.MeetingType,
                StartedAt = created.StartedAt,
                Status = created.Status
            });
        }


        [HttpPost("{callId}/join")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Unirse a una videollamada.")]
        public async Task<IActionResult> Join(int callId, [FromBody] CallJoinDto request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.Join");

            if (callId <= 0 || request.UserId <= 0)
                throw new DomainValidationException("Datos inválidos.");

            _joinCall.Execute(callId, request.UserId);

            return Ok(new { message = "Usuario unido a la llamada." });
        }

        [HttpPost("{callId}/end")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Finaliza una videollamada.")]
        public async Task<IActionResult> End(int callId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.End");

            if (callId <= 0)
                throw new DomainValidationException("ID inválido.");

            _endCall.Execute(callId);

            return Ok(new { message = "Llamada finalizada." });
        }

        [HttpGet("{callId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Obtiene datos de una llamada.")]
        public async Task<IActionResult> GetCall(int callId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.ViewDetails");

            if (callId <= 0)
                throw new DomainValidationException("ID inválido.");

            return Ok(_getCallDetails.Execute(callId));
        }

        [HttpGet("{callId}/participants")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Obtiene los participantes.")]
        public async Task<IActionResult> GetParticipants(int callId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.ViewParticipants");

            return Ok(_getCallParticipants.Execute(callId));
        }

        [HttpGet("{callId}/state")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Estado completo de la llamada.")]
        public async Task<IActionResult> GetCallState(int callId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.ViewState");

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
            await _permissionService.EnsurePermissionAsync(User, "Calls.UploadRecording");

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
        public async Task<IActionResult> SendMessage(int callId, [FromBody] SendCallMessageDto request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.SendMessage");

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
        [HttpGet("consortium/{consortiumId}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(Summary = "Obtiene la lista de reuniones de un consorcio con filtro.")]
        public async Task<IActionResult> GetByConsortium(
            int consortiumId,
            [FromQuery] string? status)
        {
            await _permissionService.EnsurePermissionAsync(User, "Calls.View");

            if (consortiumId <= 0)
                throw new DomainValidationException("ConsortiumId inválido.");

            var result = _getCallsByConsortium.Execute(consortiumId);


            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.ToLower();

                result = status switch
                {
                    "active" => result.Where(x => x.call.Status == "Active").ToList(),

                    "finished" or "finalizada" =>
                        result.Where(x => x.call.Status == "Finished" || x.call.EndedAt != null).ToList(),

                    "scheduled" or "programada" =>
                        result.Where(x => x.call.StartedAt > DateTime.Now).ToList(),

                    _ => result
                };
            }

            var dto = result.Select(x => new CallListItemDto
            {
                Id = x.call.Id,
                Title = x.call.Title,
                Description = x.call.Description,
                MeetingType = x.call.MeetingType,
                Status = x.call.Status,
                StartedAt = x.call.StartedAt,
                ParticipantsCount = x.participants,
                Location = "Virtual"
            }).ToList();

            return Ok(dto);
        }
    }
}
