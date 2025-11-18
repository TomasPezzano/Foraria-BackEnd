using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class LeaveCall
    {
        private readonly ICallParticipantRepository _participantRepository;
        private readonly ICallRepository _callRepository;

        public LeaveCall(
            ICallParticipantRepository participantRepository,
            ICallRepository callRepository)
        {
            _participantRepository = participantRepository;
            _callRepository = callRepository;
        }

        public void Execute(int callId, int userId)
        {
            var call = _callRepository.GetById(callId);
            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            _participantRepository.SetDisconnected(callId, userId);
        }
    }
}
