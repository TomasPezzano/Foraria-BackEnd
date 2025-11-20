using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetCallsByConsortium
    {
        private readonly ICallRepository _callRepo;
        private readonly ICallParticipantRepository _participantRepo;

        public GetCallsByConsortium(
            ICallRepository callRepo,
            ICallParticipantRepository participantRepo)
        {
            _callRepo = callRepo;
            _participantRepo = participantRepo;
        }

        public List<(Call call, int participants)> Execute(int consortiumId)
        {
            var calls = _callRepo.GetByConsortium(consortiumId);

            var result = new List<(Call call, int participants)>();

            foreach (var c in calls)
            {
                int count = _participantRepo.CountByCallId(c.Id);

                result.Add((call: c, participants: count));
            }

            return result;
        }
    }
}
