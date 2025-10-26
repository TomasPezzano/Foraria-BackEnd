using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class UserDocumentController : ControllerBase
{
    private readonly ICreateUserDocument _createUserDocument;
    private readonly IGetUserDocuments _getUserDocuments;

    public UserDocumentController(ICreateUserDocument createUserDocument, IGetUserDocuments getUserDocuments)
    {
        _createUserDocument = createUserDocument;
        _getUserDocuments = getUserDocuments;
    }

    /// <summary>
    /// Obtiene todos los documentos de usuario registrados.
    /// </summary>
    /// <returns>Lista de documentos con sus datos asociados.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Crea un nuevo documento de usuario.
    /// </summary>
    /// <param name="dto">Datos del documento a crear.</param>
    /// <returns>Documento creado con sus datos completos.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] CreateUserDocumentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Validaciones de existencia (si aplican)
            // Ejemplo: validar si el usuario o consorcio existen antes de crear el documento
            // var user = await _getUserById.Execute(dto.User_id);
            // if (user == null)
            //     return NotFound(new { message = "Usuario no encontrado" });

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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }
}
