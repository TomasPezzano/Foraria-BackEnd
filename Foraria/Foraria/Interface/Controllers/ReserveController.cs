using Foraria.Contracts.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

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

    /// <summary>
    /// Obtiene todas las reservas registradas.
    /// </summary>
    /// <returns>Lista de reservas.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReserveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var reserves = await _getAllReserve.Execute();
        return Ok(reserves);
    }

    /// <summary>
    /// Crea una nueva reserva.
    /// </summary>
    /// <param name="reserveDto">Datos de la reserva.</param>
    /// <returns>Reserva creada.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReserveDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add(ReserveDto reserveDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            
             var place = await _getPlaceById.Execute(reserveDto.Place_id);
             if (place == null)
                 return NotFound(new { message = "Lugar no encontrado" });

            var reserve = new Reserve
            {
                Description = reserveDto.Description,
                State = "Nuevo",
                CreatedAt = reserveDto.CreatedAt,
                Place_id = reserveDto.Place_id,
                Residence_id = reserveDto.Residence_id,
                User_id = reserveDto.User_id
            };

            var created = await _createReserve.Execute(reserve);

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

            return CreatedAtAction(nameof(GetAll), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al crear la reserva.", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de reservas antiguas.
    /// </summary>
    /// <returns>Mensaje de confirmación.</returns>
    [HttpPost("update-old")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOldReserves()
    {
        try
        {
            await _updateOldReserves.Execute();
            return Ok("Reservas viejas actualizadas correctamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar reservas antiguas", details = ex.Message });
        }
    }
}
