using ForariaDomain;
using ForariaDomain.Repository;

namespace Foraria.Application.UseCase;

public class CreateCall
{
    private readonly ICallRepository _callRepo;

    public CreateCall(ICallRepository callRepo)
    {
        _callRepo = callRepo;
    }

    public Call Execute(int userId)
    {
        var call = new Call
        {
            CreatedByUserId = userId,
            StartedAt = DateTime.Now
        };

        return _callRepo.Create(call);
    }
}
