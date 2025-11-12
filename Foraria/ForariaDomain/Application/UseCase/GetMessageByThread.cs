using Foraria.Domain.Repository.Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

public class GetMessagesByThread
{
    private readonly IMessageRepository _repository;

    public GetMessagesByThread(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Message>> Execute(int threadId)
    {
        return await _repository.GetByThread(threadId);
    }
}
