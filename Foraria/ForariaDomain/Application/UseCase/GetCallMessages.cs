using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetCallMessages
    {
        private readonly ICallRepository _callRepository;
        private readonly ICallMessageRepository _messageRepository;

        public GetCallMessages(
            ICallRepository callRepository,
            ICallMessageRepository messageRepository)
        {
            _callRepository = callRepository;
            _messageRepository = messageRepository;
        }

        public List<CallMessage> Execute(int callId)
        {
            var call = _callRepository.GetById(callId);

            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            return _messageRepository
                .GetLastByCall(callId)
                .ToList();
        }
    }
}
