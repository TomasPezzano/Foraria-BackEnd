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
public class SupplierController : ControllerBase
{
    private readonly ICreateSupplier _createSupplier;
    private readonly IDeleteSupplier _deleteSupplier;
    private readonly GetSupplierById _getSupplierById;
    private readonly IGetAllSupplier _getAllSupplier;
    private readonly IGetConsortiumById _getConsortiumById;

    public SupplierController(
        ICreateSupplier createSupplier,
        IDeleteSupplier deleteSupplier,
        GetSupplierById getSupplierById,
        IGetAllSupplier getAllSupplier,
        IGetConsortiumById getConsortiumById)
    {
        _createSupplier = createSupplier;
        _deleteSupplier = deleteSupplier;
        _getSupplierById = getSupplierById;
        _getAllSupplier = getAllSupplier;
        _getConsortiumById = getConsortiumById;
    }

    [HttpPost]
    //[Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Crea un nuevo proveedor.",
        Description = "Registra un proveedor asociado a un consorcio existente con sus datos de contacto y categoría."
    )]
    [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] SupplierRequestDto request)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos del proveedor no son válidos.");

        var consortiumExists = await _getConsortiumById.Execute(request.ConsortiumId);
        if (consortiumExists == null)
            throw new NotFoundException("El consorcio especificado no existe.");

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
            Observations = request.Observations,
            ConsortiumId = request.ConsortiumId
        };

        var createdSupplier = await _createSupplier.Execute(supplier);
        if (createdSupplier == null)
            throw new BusinessException("No se pudo crear el proveedor.");

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

    [HttpDelete("{id}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Elimina un proveedor existente.",
        Description = "Desactiva o elimina un proveedor por su ID."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
            throw new DomainValidationException("Debe especificar un ID de proveedor válido.");

        _deleteSupplier.Execute(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene un proveedor por su ID.",
        Description = "Devuelve los detalles completos de un proveedor existente."
    )]
    [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSupplierById(int id)
    {
        if (id <= 0)
            throw new DomainValidationException("Debe especificar un ID de proveedor válido.");

        var supplier = _getSupplierById.Execute(id);
        if (supplier == null)
            throw new NotFoundException($"Proveedor con ID {id} no encontrado.");

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
    [Authorize(Policy = "All")]
    [SwaggerOperation(
        Summary = "Obtiene todos los proveedores.",
        Description = "Devuelve la lista completa de proveedores registrados en el sistema."
    )]
    [ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAllSuppliers()
    {
        var suppliers = _getAllSupplier.Execute();
        if (suppliers == null || !suppliers.Any())
            throw new NotFoundException("No se encontraron proveedores registrados.");

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
