using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class SendCallMessage
    {
        private readonly ICallRepository _callRepository;
        private readonly ICallParticipantRepository _participantRepository;
        private readonly ICallMessageRepository _messageRepository;

        public SendCallMessage(
            ICallRepository callRepository,
            ICallParticipantRepository participantRepository,
            ICallMessageRepository messageRepository)
        {
            _callRepository = callRepository;
            _participantRepository = participantRepository;
            _messageRepository = messageRepository;
        }

        public CallMessage Execute(int callId, int userId, string text)
        {
            var call = _callRepository.GetById(callId);

            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            var isParticipant = _participantRepository.IsUserInCall(callId, userId);

            if (!isParticipant)
                throw new ForbiddenAccessException("El usuario no pertenece a esta llamada.");

            if (string.IsNullOrWhiteSpace(text))
                throw new DomainValidationException("El mensaje no puede estar vacío.");

            var message = new CallMessage
            {
                CallId = callId,
                UserId = userId,
                Message = text,
                SentAt = DateTime.UtcNow
            };

            _messageRepository.Save(message);

            return message; 
        }
    }
}
