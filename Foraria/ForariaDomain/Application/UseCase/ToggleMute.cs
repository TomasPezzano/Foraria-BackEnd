using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class ToggleMute
    {
        private readonly ICallParticipantRepository _participantRepository;
        private readonly ICallRepository _callRepository;

        public ToggleMute(
            ICallParticipantRepository participantRepository,
            ICallRepository callRepository)
        {
            _participantRepository = participantRepository;
            _callRepository = callRepository;
        }

        public void Execute(int callId, int userId, bool isMuted)
        {
            var call = _callRepository.GetById(callId);
            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            _participantRepository.SetMute(callId, userId, isMuted);
        }
    }
}
