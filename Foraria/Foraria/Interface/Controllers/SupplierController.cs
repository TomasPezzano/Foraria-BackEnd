using Foraria.Contracts.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ICreateSupplier _createSupplier;
    private readonly IDeleteSupplier _deleteSupplier;
    public SupplierController(ICreateSupplier createSupplier, IDeleteSupplier deleteSupplier)
    {
        _createSupplier = createSupplier;
        _deleteSupplier = deleteSupplier;
    }

    [HttpPost]
    public IActionResult Create([FromBody] SupplierRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = new Supplier
            {
                CommercialName = request.CommercialName,
                BusinessName = request.BusinessName,
                Cuit = request.Cuit,
                SupplierCategory = request.supplierCategory,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                ContactPerson = request.ContactPerson,
                Observations = request.Observations
            };

            var createdSupplier = _createSupplier.Execute(supplier);

            var response = new SupplierResponseDto
            {
                Id = createdSupplier.Id,
                CommercialName = createdSupplier.CommercialName,
                BusinessName = createdSupplier.BusinessName,
                Cuit = createdSupplier.Cuit,
                SupplierCategory = createdSupplier.SupplierCategory,
                Email = createdSupplier.Email,
                Phone = createdSupplier.Phone,
                Address = createdSupplier.Address,
                ContactPerson = createdSupplier.ContactPerson,
                Active = createdSupplier.Active,
                RegistrationDate = createdSupplier.RegistrationDate
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

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            _deleteSupplier.Execute(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al eliminar el proveedor.", error = ex.Message });
        }
    }


}
