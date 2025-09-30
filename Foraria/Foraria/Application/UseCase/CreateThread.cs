using Foraria.Domain.Repository;
using ForariaDomain;
using Thread = ForariaDomain.Thread; //la entidad Thread choca con System.Threading.Thread


namespace Foraria.Application.UseCase
{
    public class CreateThread
    {
        private readonly IThreadRepository _repository;

        public CreateThread(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<Thread> Execute(Thread thread)
        {
            thread.CreatedAt = DateTime.UtcNow;
            thread.State = "Active";

            await _repository.Add(thread);
            return thread;
        }
    }
}
