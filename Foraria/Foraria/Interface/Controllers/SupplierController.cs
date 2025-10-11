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
    private readonly GetSupplierById _getSupplierById;
    private readonly IGetAllSupplier _getAllSupplier;
    public SupplierController(ICreateSupplier createSupplier, IDeleteSupplier deleteSupplier, GetSupplierById getSupplierById, IGetAllSupplier getAllSupplier)
    {
        _createSupplier = createSupplier;
        _deleteSupplier = deleteSupplier;
        _getSupplierById = getSupplierById;
        _getAllSupplier = getAllSupplier;
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

    [HttpGet("{id}")]
    public IActionResult GetSupplierById(int id)
    {
        var supplier = _getSupplierById.Execute(id);

        if (supplier == null)
        {
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });
        }

        var response = new SupplierResponseDto
        {
            Id = supplier.Id,
            BusinessName = supplier.BusinessName,
            Observations = supplier.Observations,
            Cuit = supplier.Cuit,
            CommercialName = supplier.CommercialName,
            SupplierCategory = supplier.SupplierCategory,
            Active = supplier.Active,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            ContactPerson = supplier.ContactPerson,
            RegistrationDate = supplier.RegistrationDate,
        };

        return Ok(response);

    }

    [HttpGet]
    public IActionResult GetAllSuppliers()
    {
        var suppliers = _getAllSupplier.Execute();
        var response = suppliers.Select(s => new SupplierResponseDto
        {
            Id = s.Id,
            BusinessName = s.BusinessName,
            Observations = s.Observations,
            Cuit = s.Cuit,
            CommercialName = s.CommercialName,
            SupplierCategory = s.SupplierCategory,
            Active = s.Active,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            ContactPerson = s.ContactPerson,
            RegistrationDate = s.RegistrationDate,
        }).ToList();
        return Ok(response);

    }

}
