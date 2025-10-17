using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface ICreateUserDocument
{
    Task<UserDocument> Execute(UserDocument document);
}

public class CreateUserDocument : ICreateUserDocument
{
    private readonly IUserDocumentRepository _userDocumentRepository;

    public CreateUserDocument(IUserDocumentRepository userDocumentRepository)
    {
        _userDocumentRepository = userDocumentRepository;
    }

    public async Task<UserDocument> Execute(UserDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.Title))
            throw new ArgumentException("El título del documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(document.Category))
            throw new ArgumentException("La categoría del documento es obligatoria.");

        if (document.User_id <= 0)
            throw new ArgumentException("Debe asociarse un usuario válido al documento.");

        if (document.Consortium_id <= 0)
            throw new ArgumentException("Debe asociarse un consorcio válido al documento.");

        document.CreatedAt = DateTime.UtcNow;

        await _userDocumentRepository.Add(document);

        return document;
    }
}

