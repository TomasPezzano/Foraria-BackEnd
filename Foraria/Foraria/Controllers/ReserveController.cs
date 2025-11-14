    using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ReserveController : ControllerBase
{
    public readonly ICreateReserve _createReserve;
    public readonly IGetAllReserve _getAllReserve;
    private readonly IUpdateOldReserves _updateOldReserves;
    private readonly IGetPlaceById _getPlaceById;

    public ReserveController(
        ICreateReserve createReserve,
        IGetAllReserve getAllReserve,
        IUpdateOldReserves updateOldReserves,
        IGetPlaceById getPlaceById)
    {
        _createReserve = createReserve;
        _getAllReserve = getAllReserve;
        _updateOldReserves = updateOldReserves;
        _getPlaceById = getPlaceById;
    }

    [HttpGet("{idConsortium}")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene todas las reservas registradas.",
        Description = "Devuelve una lista completa de reservas activas y pasadas del sistema."
    )]
    [ProducesResponseType(typeof(List<ReserveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(int idConsortium)
    {
        var reserves = await _getAllReserve.Execute(idConsortium);

        var reservesDto = new List<ReserveResponseDto>();

        foreach (var reserve in reserves)
        {
            var reserveDto = new ReserveResponseDto
            {
                Id = reserve.Id,
                Description = reserve.Description,
                State = reserve.State,
                CreatedAt = reserve.Date,
                DeletedAt = reserve.DeletedAt,
                Place_id = reserve.Place_id,
                PlaceName = reserve.Place?.Name,
                Residence_id = reserve.Residence_id,
                User_id = reserve.User_id,
                UserName = reserve.User?.Name,
                DateReserve = reserve.CreatedAt
            };

            reservesDto.Add(reserveDto);
        }

        if (reserves == null || !reserves.Any())
            throw new NotFoundException($"No se encontraron reservas para el consorcio con ID {idConsortium}.");

        return Ok(reservesDto);
    }

    [HttpPost]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Crea una nueva reserva.",
        Description = "Registra una reserva asociada a un lugar, usuario y residencia."
    )]
    [ProducesResponseType(typeof(ReserveResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add(ReserveDto reserveDto)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la reserva no son válidos.");

        if (reserveDto.Place_id <= 0)
            throw new DomainValidationException("Debe especificar un ID de lugar válido.");

        if (reserveDto.Consortium_id <= 0)
            throw new DomainValidationException("Debe especificar un ID de consorcio válido.");

        var place = await _getPlaceById.Execute(reserveDto.Place_id);
        if (place == null)
            throw new NotFoundException("Lugar no encontrado.");

        var reserve = new Reserve
        {
            Description = reserveDto.Description,
            State = "Nuevo",
            CreatedAt = reserveDto.CreatedAt,
            Place_id = reserveDto.Place_id,
            Residence_id = reserveDto.Residence_id,
            ConsortiumId = reserveDto.Consortium_id,
            User_id = reserveDto.User_id
        };

        var created = await _createReserve.Execute(reserve);
        if (created == null)
            throw new BusinessException("No se pudo crear la reserva.");

        var response = new ReserveResponseDto
        {
            Id = created.Id,
            Description = created.Description,
            State = created.State,
            CreatedAt = created.Date,
            DeletedAt = created.DeletedAt,
            Place_id = created.Place_id,
            PlaceName = created.Place?.Name,
            Residence_id = created.Residence_id,
            User_id = created.User_id,
            UserName = created.User?.Name,
            DateReserve = created.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost("update-old")]
    [SwaggerOperation(
        Summary = "Actualiza el estado de reservas antiguas.",
        Description = "Ejecuta una rutina para marcar como caducadas las reservas cuyo periodo ya finalizó."
    )]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOldReserves()
    {
        await _updateOldReserves.Execute();
        return Ok("Reservas viejas actualizadas correctamente.");
    }
}
