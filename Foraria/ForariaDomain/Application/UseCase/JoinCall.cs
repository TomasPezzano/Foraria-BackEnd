using ForariaDomain;
using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public class JoinCall
{
    private readonly ICallRepository _callRepo;
    private readonly ICallParticipantRepository _participantRepo;

    public JoinCall(ICallRepository callRepo, ICallParticipantRepository participantRepo)
    {
        _callRepo = callRepo;
        _participantRepo = participantRepo;
    }

    public void Execute(int callId, int userId)
    {
        var call = _callRepo.GetById(callId)
            ?? throw new NotFoundException("Llamada no encontrada");

        if (call.Status != "Active")
            throw new InvalidOperationException("No te puedes unir a una llamada que no esta activa.");

        var participant = new CallParticipant
        {
            CallId = callId,
            UserId = userId,
            JoinedAt = DateTime.Now
        };

        _participantRepo.Add(participant);
    }
}
