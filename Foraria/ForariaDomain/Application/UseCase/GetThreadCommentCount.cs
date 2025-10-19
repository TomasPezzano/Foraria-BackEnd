using Foraria.Domain.Repository;

namespace Foraria.Application.UseCase
{
    public class GetThreadCommentCount
    {
        private readonly IThreadRepository _repository;

        public GetThreadCommentCount(IThreadRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Execute(int threadId)
        {
            var thread = await _repository.GetById(threadId);
            if (thread == null)
                throw new InvalidOperationException($"No se encontró el hilo con ID {threadId}");

            return thread.Messages?.Count ?? 0;
        }
    }
}
