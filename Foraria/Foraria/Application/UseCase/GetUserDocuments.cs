using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface IGetUserDocuments
{
    Task<List<UserDocument>> Execute();
}
public class GetUserDocuments : IGetUserDocuments
{

    private readonly IUserDocumentRepository _userDocumentRepository;
    private readonly IGetUserDocuments _getUserDocument;

    public GetUserDocuments(IUserDocumentRepository userDocumentRepository, IGetUserDocuments getUserDocument)
    {
        _userDocumentRepository = userDocumentRepository;
        _getUserDocument = getUserDocument;
    }

    public async Task<List<UserDocument>> Execute()
    {
        var documents = await _userDocumentRepository.GetAll();

        if (documents == null || !documents.Any())
            return new List<UserDocument>();

        return documents;
    }
}
