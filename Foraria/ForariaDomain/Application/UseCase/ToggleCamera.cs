using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class ToggleCamera
    {
        private readonly ICallParticipantRepository _participantRepository;
        private readonly ICallRepository _callRepository;

        public ToggleCamera(
            ICallParticipantRepository participantRepository,
            ICallRepository callRepository)
        {
            _participantRepository = participantRepository;
            _callRepository = callRepository;
        }

        public void Execute(int callId, int userId, bool isCameraOn)
        {
            var call = _callRepository.GetById(callId);
            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            _participantRepository.SetCamera(callId, userId, isCameraOn);
        }
    }
}
