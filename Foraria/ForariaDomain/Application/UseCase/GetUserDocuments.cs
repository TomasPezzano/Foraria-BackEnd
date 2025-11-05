using Foraria.Domain.Repository;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface IGetUserDocuments
{
    Task<List<UserDocument>> Execute();
}
public class GetUserDocuments : IGetUserDocuments
{

    private readonly IUserDocumentRepository _userDocumentRepository;

    public GetUserDocuments(IUserDocumentRepository userDocumentRepository)
    {
        _userDocumentRepository = userDocumentRepository;
    }

    public async Task<List<UserDocument>> Execute()
    {
        var documents = await _userDocumentRepository.GetAll();

        if (documents == null || !documents.Any())
            return new List<UserDocument>();

        return documents;
    }
}
