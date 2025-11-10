using Foraria.Domain.Repository;


namespace ForariaDomain.Application.UseCase;

public class GetUserDocumentsByCategory
{
    private readonly IUserDocumentRepository _repository;

    public GetUserDocumentsByCategory(IUserDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserDocument>> ExecuteAsync(string category, int? userId = null)
    {
        return await _repository.GetByCategoryAsync(category, userId);
    }
}
