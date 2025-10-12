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

    public ReserveController(ICreateReserve createReserve)
    {
        _createReserve = createReserve;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok("GetAll no implementado");
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

            var createdSupplier = _createReserve.Execute(reserve);

            var response = new SupplierResponseDto
            {

            };

            return Ok(response);

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

}
