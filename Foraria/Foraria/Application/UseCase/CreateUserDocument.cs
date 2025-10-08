using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;

namespace Foraria.Application.UseCase;

public interface ICreateUserDocument
{
    Task<UserDocument> Execute(CreateUserDocumentDto documentDto);
}
public class CreateUserDocument : ICreateUserDocument
{
    private readonly IUserDocumentRepository _userDocumentRepository;
    private readonly ICreateUserDocument _createUserDocument;

    public CreateUserDocument(IUserDocumentRepository userDocumentRepository, ICreateUserDocument createUserDocument)
    {
        _userDocumentRepository = userDocumentRepository;
        _createUserDocument = createUserDocument;
    }

    public async Task<UserDocument> Execute(CreateUserDocumentDto documentDto)
    {
        if (string.IsNullOrWhiteSpace(documentDto.Title))
            throw new ArgumentException("El título del documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(documentDto.Category))
            throw new ArgumentException("La categoría del documento es obligatoria.");

        if (documentDto.User_id <= 0)
            throw new ArgumentException("Debe asociarse un usuario válido al documento.");

        if (documentDto.Consortium_id <= 0)
            throw new ArgumentException("Debe asociarse un consorcio válido al documento.");

        var document = new UserDocument
        {
            Title = documentDto.Title,
            Description = documentDto.Description,
            Category = documentDto.Category,
            CreatedAt = DateTime.UtcNow,
            Url = documentDto.Url,
            User_id = documentDto.User_id,
            Consortium_id = documentDto.Consortium_id
        };

        await _userDocumentRepository.Add(document);

        return document;
    }
}

