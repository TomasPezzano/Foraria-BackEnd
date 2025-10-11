using Foraria.Contracts.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")] 
[ApiController]
public class SupplierContractController : ControllerBase
{
    private readonly ICreateSupplierContract _createSupplierContract;
    private readonly GetSupplierById _getSupplierById;
    private readonly IFileStorageService _fileStorageService;
    private readonly GetSupplierContractById _getSupplierContractById;
    private readonly GetSupplierContractsById _getContractsBySupplierId;
    private static readonly string[] AllowedFileExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public SupplierContractController(ICreateSupplierContract createSupplierContract, GetSupplierById getSupplierById, IFileStorageService fileStorageService, GetSupplierContractById getSupplierContractById, GetSupplierContractsById getSupplierContractsBySupplierId)
    {
        _createSupplierContract = createSupplierContract;
        _getSupplierById = getSupplierById;
        _fileStorageService = fileStorageService;
        _getSupplierContractById = getSupplierContractById;
        _getContractsBySupplierId = getSupplierContractsBySupplierId;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] SupplierContractRequestDto request, IFormFile? file)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? filePath = null;
            if (file != null)
            {
                if (!await _fileStorageService.ValidateFileAsync(file, AllowedFileExtensions, MaxFileSizeInBytes))
                {
                    return BadRequest(new { message = "Archivo inválido. Formatos permitidos: PDF, DOC, DOCX, JPG, PNG. Tamaño máximo: 10MB." });
                }

                filePath = await _fileStorageService.SaveFileAsync(file, "contracts");
            }

            var contract = new SupplierContract
            {
                Name = request.Name,
                ContractType = request.ContractType,
                Description = request.Description,
                MonthlyAmount = request.MonthlyAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SupplierId = request.SupplierId,
                FilePath = filePath 
            };

            var createdContract = _createSupplierContract.Execute(contract);

            var supplier = _getSupplierById.Execute(createdContract.SupplierId);

            var response = new SupplierContractResponseDto
            {
                Id = createdContract.Id,
                Name = createdContract.Name,
                ContractType = createdContract.ContractType,
                Description = createdContract.Description,
                MonthlyAmount = createdContract.MonthlyAmount,
                StartDate = createdContract.StartDate,
                EndDate = createdContract.EndDate,
                Active = createdContract.Active,
                FilePath = createdContract.FilePath != null
                    ? await _fileStorageService.GetFileUrlAsync(createdContract.FilePath)
                    : null,
                SupplierId = createdContract.SupplierId,
                SupplierName = supplier?.CommercialName ?? "Desconocido",
                CreatedAt = createdContract.CreatedAt
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al crear el contrato.", error = ex.Message });
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var contract = _getSupplierContractById.Execute(id);

            if (contract == null)
            {
                return NotFound(new { message = $"Contrato con ID {id} no encontrado." });
            }

            var supplier = _getSupplierById.Execute(contract.SupplierId);

            var response = new SupplierContractResponseDto
            {
                Id = contract.Id,
                Name = contract.Name,
                ContractType = contract.ContractType,
                Description = contract.Description,
                MonthlyAmount = contract.MonthlyAmount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Active = contract.Active,
                FilePath = contract.FilePath != null
                    ? await _fileStorageService.GetFileUrlAsync(contract.FilePath)
                    : null,
                SupplierId = contract.SupplierId,
                SupplierName = supplier?.CommercialName ?? "Desconocido",
                CreatedAt = contract.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al obtener el contrato.", error = ex.Message });
        }
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplierId(int supplierId)
    {
        try
        {
            var supplier = _getSupplierById.Execute(supplierId);
            if (supplier == null)
            {
                return NotFound(new { message = $"Proveedor con ID {supplierId} no encontrado." });
            }

            var contracts = _getContractsBySupplierId.Execute(supplierId);

            var response = new List<SupplierContractResponseDto>();

            foreach (var contract in contracts)
            {
                response.Add(new SupplierContractResponseDto
                {
                    Id = contract.Id,
                    Name = contract.Name,
                    ContractType = contract.ContractType,
                    Description = contract.Description,
                    MonthlyAmount = contract.MonthlyAmount,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Active = contract.Active,
                    FilePath = contract.FilePath != null
                        ? await _fileStorageService.GetFileUrlAsync(contract.FilePath)
                        : null,
                    SupplierId = contract.SupplierId,
                    SupplierName = supplier.CommercialName,
                    CreatedAt = contract.CreatedAt
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al obtener los contratos.", error = ex.Message });
        }
    }

}
