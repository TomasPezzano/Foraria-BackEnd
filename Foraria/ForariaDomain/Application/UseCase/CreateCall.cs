using ForariaDomain;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase
{
    public class CreateCall
    {
        private readonly ICallRepository _callRepo;

        public CreateCall(ICallRepository callRepo)
        {
            _callRepo = callRepo;
        }

        public Call Execute(Call call)
        {
            call.StartedAt = DateTime.Now;
            call.Status = "Active";

            return _callRepo.Create(call);
        }
    }
}
