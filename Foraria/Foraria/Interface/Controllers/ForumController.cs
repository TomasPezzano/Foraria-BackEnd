using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly CreateForum _createForum;
        private readonly GetForumById _getForumById;
        private readonly GetAllForums _getAllForums;
        private readonly GetForumWithThreads _getForumWithThreads;
        private readonly DeleteForum _deleteForum;
        private readonly GetForumWithCategory _getForumWithCategory;

        public ForumController(
            CreateForum createForum,
            GetForumById getForumById,
            GetAllForums getAllForums,
            GetForumWithThreads getForumWithThreads,
            DeleteForum deleteForum,
            GetForumWithCategory getForumWithCategory)
        {
            _createForum = createForum;
            _getForumById = getForumById;
            _getAllForums = getAllForums;
            _getForumWithThreads = getForumWithThreads;
            _deleteForum = deleteForum;
            _getForumWithCategory = getForumWithCategory;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crea un nuevo foro.",
            Description = "Permite crear un foro dentro de una categoría específica. Devuelve los datos del foro creado si la operación es exitosa."
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateForumRequest request)
        {
            if (request == null)
                throw new ValidationException("El cuerpo de la solicitud no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(request.Category.ToString()))
                throw new ValidationException("Debe especificar una categoría válida para el foro.");

            var createdForum = await _createForum.Execute(request);

            if (createdForum == null)
                throw new BusinessException("No se pudo crear el foro. Verifique los datos ingresados.");

            return CreatedAtAction(nameof(GetById), new { id = createdForum.Id }, createdForum);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtiene un foro por su ID.",
            Description = "Devuelve los detalles del foro solicitado, incluyendo su categoría, siempre que el ID sea válido."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var response = await _getForumById.Execute(id);

            if (response == null)
                throw new NotFoundException($"No se encontró el foro con ID {id}.");

            return Ok(response);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene todos los foros disponibles.",
            Description = "Devuelve una lista de todos los foros registrados en el sistema, incluyendo su categoría."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var responses = await _getAllForums.Execute();

            if (responses == null || !responses.Any())
                throw new NotFoundException("No se encontraron foros disponibles.");

            return Ok(responses);
        }

        [HttpGet("{id}/threads")]
        [SwaggerOperation(
            Summary = "Obtiene un foro junto con sus hilos de discusión.",
            Description = "Devuelve el foro solicitado con la lista completa de hilos y sus mensajes asociados."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForumWithThreads(int id)
        {
            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var forum = await _getForumWithThreads.Execute(id);

            if (forum == null)
                throw new NotFoundException($"No se encontró el foro con ID {id}.");

            return Ok(forum);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Deshabilita un foro existente.",
            Description = "Marca un foro como deshabilitado sin eliminarlo de la base de datos. Los hilos asociados permanecen inactivos."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(int id)
        {
            await _deleteForum.Execute(id);
            return Ok(new { message = "Foro deshabilitado correctamente." });
        }

        [HttpGet("{id}/with-category")]
        [SwaggerOperation(
            Summary = "Obtiene un foro con su categoría.",
            Description = "Devuelve los datos del foro junto con la información detallada de su categoría asignada."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForumWithCategory(int id)
        {
            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var forum = await _getForumWithCategory.Execute(id);

            if (forum == null)
                throw new NotFoundException($"No se encontró el foro con ID {id}.");

            return Ok(forum);
        }
    }
}
