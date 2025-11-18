using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class SaveCallMessage
    {
        private readonly ICallRepository _callRepository;
        private readonly ICallMessageRepository _messageRepository;

        public SaveCallMessage(
            ICallRepository callRepository,
            ICallMessageRepository messageRepository)
        {
            _callRepository = callRepository;
            _messageRepository = messageRepository;
        }

        public void Execute(int callId, int userId, string message)
        {
            var call = _callRepository.GetById(callId);
            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            var msg = new CallMessage
            {
                CallId = callId,
                UserId = userId,
                Message = message,
                SentAt = DateTime.Now
            };

            _messageRepository.Save(msg);
        }
    }
}
