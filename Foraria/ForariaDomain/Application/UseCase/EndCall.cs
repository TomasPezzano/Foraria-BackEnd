using ForariaDomain.Exceptions;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public class EndCall
{
    private readonly ICallRepository _callRepo;

    public EndCall(ICallRepository callRepo)
    {
        _callRepo = callRepo;
    }

    public void Execute(int callId)
    {
        var call = _callRepo.GetById(callId)
            ?? throw new NotFoundException("Llamada no encontrada");

        call.Status = "Ended";
        call.EndedAt = DateTime.UtcNow;

        _callRepo.Update(call);
    }
}
