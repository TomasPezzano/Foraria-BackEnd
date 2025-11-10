using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Controllers
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
        private readonly IPermissionService _permissionService;

        public ForumController(
            CreateForum createForum,
            GetForumById getForumById,
            GetAllForums getAllForums,
            GetForumWithThreads getForumWithThreads,
            DeleteForum deleteForum,
            GetForumWithCategory getForumWithCategory,
            IPermissionService permissionService)
        {
            _createForum = createForum;
            _getForumById = getForumById;
            _getAllForums = getAllForums;
            _getForumWithThreads = getForumWithThreads;
            _deleteForum = deleteForum;
            _getForumWithCategory = getForumWithCategory;
            _permissionService = permissionService;
        }


        [HttpPost]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Crea un nuevo foro.",
            Description = "Permite crear un foro dentro de una categoría específica. Devuelve los datos del foro creado si la operación es exitosa."
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateForumRequest request)
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.Create");

            if (request == null)
                throw new ValidationException("El cuerpo de la solicitud no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(request.Category.ToString()))
                throw new ValidationException("Debe especificar una categoría válida para el foro.");

            var forum = new Forum
            {
                Category = request.Category
            };

            var createdForum = await _createForum.Execute(forum);

            var response = new ForumResponse
            {
                Id = createdForum.Id,
                Category = createdForum.Category
            };

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un foro por su ID.",
            Description = "Devuelve los detalles del foro solicitado, incluyendo su categoría, siempre que el ID sea válido."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.View");

            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var forum = await _getForumById.Execute(id);

            var response = new ForumResponse
            {
                Id = forum.Id,
                Category = forum.Category,
                CategoryName = forum.Category.ToString(),
                CountThreads = forum.CountThreads,
                CountResponses = forum.CountResponses,
                CountUserActives = forum.CountUserActives
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene todos los foros disponibles.",
            Description = "Devuelve una lista de todos los foros registrados en el sistema, incluyendo su categoría."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.ViewAll");

            var forums = await _getAllForums.Execute();

            var responses = forums.Select(f => new ForumResponse
            {
                Id = f.Id,
                Category = f.Category,
                CategoryName = f.Category.ToString()
            });

            return Ok(responses);
        }

        [HttpGet("{id}/threads")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un foro junto con sus hilos de discusión.",
            Description = "Devuelve el foro solicitado con la lista completa de hilos y sus mensajes asociados."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForumWithThreads(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.ViewThreads");

            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var forum = await _getForumWithThreads.Execute(id);

            var response = new ForumDto
            {
                Id = forum.Id,
                Category = forum.Category,
                Threads = forum.Threads.Select(t => new ThreadDto
                {
                    Id = t.Id,
                    Theme = t.Theme,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    State = t.State,
                    UserId = t.UserId
                }).ToList()
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize (Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Deshabilita un foro existente.",
            Description = "Marca un foro como deshabilitado sin eliminarlo de la base de datos. Los hilos asociados permanecen inactivos."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Disable(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.Disable");

            await _deleteForum.Execute(id);
            return Ok(new { message = "Foro deshabilitado correctamente." });
        }

        [HttpGet("{id}/with-category")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene un foro con su categoría.",
            Description = "Devuelve los datos del foro junto con la información detallada de su categoría asignada."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForumWithCategory(int id)
        {
            await _permissionService.EnsurePermissionAsync(User, "Forums.ViewWithCategory");

            if (id <= 0)
                throw new ValidationException("El ID del foro debe ser mayor que cero.");

            var forum = await _getForumWithCategory.Execute(id);

            var response = new ForumWithCategoryDto
            {
                Id = forum.Id,
                Category = forum.Category,
                CategoryName = forum.Category.ToString(),
                IsActive = forum.IsActive,
            };

            return Ok(response);
        }
    }
}
