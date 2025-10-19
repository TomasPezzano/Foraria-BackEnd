using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserDocumentController : ControllerBase
{
    private readonly ICreateUserDocument _createUserDocument;
    private readonly IGetUserDocuments _getUserDocuments;

    public UserDocumentController(ICreateUserDocument createUserDocument, IGetUserDocuments getUserDocuments)
    {
        _createUserDocument = createUserDocument;
        _getUserDocuments = getUserDocuments;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var documents = await _getUserDocuments.Execute();

        var result = documents.Select(d => new UserDocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Description = d.Description,
            Category = d.Category,
            CreatedAt = d.CreatedAt,
            Url = d.Url,
            User_id = d.User_id,
            Consortium_id = d.Consortium_id
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateUserDocumentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var document = new UserDocument
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Url = dto.Url,
                User_id = dto.User_id,
                Consortium_id = dto.Consortium_id
            };

            var createdDocument = await _createUserDocument.Execute(document);

            var result = new UserDocumentDto
            {
                Id = createdDocument.Id,
                Title = createdDocument.Title,
                Description = createdDocument.Description,
                Category = createdDocument.Category,
                CreatedAt = createdDocument.CreatedAt,
                Url = createdDocument.Url,
                User_id = createdDocument.User_id,
                Consortium_id = createdDocument.Consortium_id
            };

            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
