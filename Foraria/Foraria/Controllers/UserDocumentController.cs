using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class UserDocumentController : ControllerBase
{
    private readonly ICreateUserDocument _createUserDocument;
    private readonly IGetUserDocuments _getUserDocuments;
    private readonly UpdateUserDocument _updateUserDocument;
    private readonly GetUserDocumentsByCategory _getUserDocumentsByCategory;
    private readonly GetLastUploadDate _getLastUploadDate;
    private readonly GetUserDocumentStats _getUserDocumentStats;

    public UserDocumentController(
        ICreateUserDocument createUserDocument,
        IGetUserDocuments getUserDocuments,
        UpdateUserDocument updateUserDocument,
        GetUserDocumentsByCategory getUserDocumentsByCategory,
        GetLastUploadDate getLastUploadDate,
        GetUserDocumentStats getUserDocumentStats)
    {
        _createUserDocument = createUserDocument;
        _getUserDocuments = getUserDocuments;
        _updateUserDocument = updateUserDocument;
        _getUserDocumentsByCategory = getUserDocumentsByCategory;
        _getLastUploadDate = getLastUploadDate;
        _getUserDocumentStats = getUserDocumentStats;
    }

    [HttpGet]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene todos los documentos existentes",
        Description = "Devuelve la lista completa de documentos de usuario y consorcio.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de documentos obtenida correctamente", typeof(List<UserDocumentDto>))]
    public async Task<IActionResult> GetAll()
    {
        var documents = await _getUserDocuments.Execute();
        if (documents == null || !documents.Any())
            throw new NotFoundException("No se encontraron documentos registrados.");

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
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Crea un nuevo documento de usuario",
        Description = "Permite subir un documento validado por URL, categoría y formato. Cualquier usuario autenticado puede crear un documento.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Documento creado correctamente", typeof(UserDocumentDto))]
    public async Task<IActionResult> Add([FromBody] CreateUserDocumentDto dto)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos del documento son inválidos.");

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
        if (createdDocument == null)
            throw new BusinessException("No se pudo crear el documento.");

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

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Consorcio,Usuario")]
    [SwaggerOperation(
        Summary = "Actualiza un documento existente",
        Description = "Permite modificar los datos de un documento si el usuario es el propietario, un administrador o un consorcio.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Documento actualizado correctamente", typeof(UserDocumentDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "El usuario no tiene permisos para modificar este documento")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No se encontró el documento especificado")]
    public async Task<IActionResult> UpdateUserDocument(int id, [FromBody] UpdateUserDocumentRequest request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Datos inválidos para actualización de documento.");

        var updated = await _updateUserDocument.ExecuteAsync(
            id,
            request.UserId,
            request.Title,
            request.Description,
            request.Category,
            request.Url
        );

        if (updated == null)
            throw new NotFoundException("No se pudo actualizar el documento o no existe.");

        var dto = new UserDocumentDto
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Category = updated.Category,
            CreatedAt = updated.CreatedAt,
            Url = updated.Url,
            User_id = updated.User_id,
            Consortium_id = updated.Consortium_id
        };

        return Ok(dto);
    }

    [HttpGet("category/{category}")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Filtra documentos por categoría",
        Description = "Devuelve los documentos de una categoría específica, opcionalmente filtrados por usuario.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Documentos filtrados correctamente", typeof(List<UserDocumentDto>))]
    public async Task<IActionResult> GetByCategory(string category, [FromQuery] int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainValidationException("Debe especificar una categoría válida.");

        var documents = await _getUserDocumentsByCategory.ExecuteAsync(category, userId);
        if (documents == null || !documents.Any())
            throw new NotFoundException("No se encontraron documentos en la categoría indicada.");

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

    [HttpGet("last-upload")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene la fecha del último documento subido",
        Description = "Devuelve la fecha del último documento subido por un usuario o globalmente.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Fecha obtenida correctamente", typeof(DateTime))]
    public async Task<IActionResult> GetLastUpload([FromQuery] int? userId = null)
    {
        var date = await _getLastUploadDate.ExecuteAsync(userId);
        if (date == null)
            throw new NotFoundException("No se encontraron documentos para calcular la última carga.");

        return Ok(date);
    }

    [HttpGet("stats")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene estadísticas de documentos",
        Description = "Devuelve totales, totales por categoría y fecha de la última carga de documentos.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Estadísticas obtenidas correctamente", typeof(UserDocumentStatsDto))]
    public async Task<IActionResult> GetStats([FromQuery] int? userId = null)
    {
        var stats = await _getUserDocumentStats.ExecuteAsync(userId);
        if (stats == null)
            throw new NotFoundException("No se pudieron calcular las estadísticas.");

        return Ok(stats);
    }
}
