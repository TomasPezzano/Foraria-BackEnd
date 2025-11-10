using Foraria.Domain.Repository.Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

public class GetMessagesByUser
{
    private readonly IMessageRepository _repository;

    public GetMessagesByUser(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Message>> ExecuteAsync(int userId)
    {
        return await _repository.GetByUserIdAsync(userId);


    }
}
