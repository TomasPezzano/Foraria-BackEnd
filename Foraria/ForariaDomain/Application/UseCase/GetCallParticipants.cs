using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetCallParticipants
    {
        private readonly ICallParticipantRepository _participantRepository;
        private readonly ICallRepository _callRepository;

        public GetCallParticipants(
            ICallParticipantRepository participantRepository,
            ICallRepository callRepository)
        {
            _participantRepository = participantRepository;
            _callRepository = callRepository;
        }

        public IEnumerable<CallParticipant> Execute(int callId)
        {
            var call = _callRepository.GetById(callId);

            if (call == null)
                throw new NotFoundException("La llamada no existe.");

            return _participantRepository
                .GetParticipants(callId)
                .ToList();
        }
    }
}
