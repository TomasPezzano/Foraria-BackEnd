using Foraria.Contracts.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReserveController : ControllerBase
{

    public readonly ICreateReserve _createReserve;
    public readonly IGetAllReserve _getAllReserve;
    private readonly IUpdateOldReserves _updateOldReserves;

    public ReserveController(
        ICreateReserve createReserve,
        IGetAllReserve getAllReserve,
        IUpdateOldReserves updateOldReserves)
    {
        _createReserve = createReserve;
        _getAllReserve = getAllReserve;
        _updateOldReserves = updateOldReserves;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reserves = await _getAllReserve.Execute();
        return Ok(reserves);
    }

    [HttpPost]
    public async Task<IActionResult> Add(ReserveRequestDto reserveDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            return Ok(created);

        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al crear el proveedor." });
        }

    }

    [HttpPost("update-old")]
    public async Task<IActionResult> UpdateOldReserves()
    {
        await _updateOldReserves.Execute();
        return Ok("Reservas viejas actualizadas correctamente.");
    }

}
