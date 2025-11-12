using Foraria.Domain.Repository;

namespace ForariaDomain.Application.UseCase;

public class GetLastUploadDate
{
    private readonly IUserDocumentRepository _repository;

    public GetLastUploadDate(IUserDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DateTime?> ExecuteAsync(int? userId = null)
    {
        return await _repository.GetLastUploadDateAsync(userId);
    }
}
