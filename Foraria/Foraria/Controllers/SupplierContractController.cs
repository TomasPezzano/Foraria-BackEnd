using Foraria.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SupplierContractController : ControllerBase
{
    private readonly ICreateSupplierContract _createSupplierContract;
    private readonly GetSupplierById _getSupplierById;
    private readonly ILocalFileStorageService _localFileStorageService;
    private readonly GetSupplierContractById _getSupplierContractById;
    private readonly GetSupplierContractsById _getContractsBySupplierId;
    private static readonly string[] AllowedFileExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public SupplierContractController(
        ICreateSupplierContract createSupplierContract,
        GetSupplierById getSupplierById,
        ILocalFileStorageService localFileStorageService,
        GetSupplierContractById getSupplierContractById,
        GetSupplierContractsById getSupplierContractsBySupplierId)
    {
        _createSupplierContract = createSupplierContract;
        _getSupplierById = getSupplierById;
        _localFileStorageService = localFileStorageService;
        _getSupplierContractById = getSupplierContractById;
        _getContractsBySupplierId = getSupplierContractsBySupplierId;
    }

    [HttpPost]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Crea un nuevo contrato de proveedor.",
        Description = "Registra un contrato de proveedor, validando el archivo adjunto (PDF, DOC, DOCX, JPG, PNG) y almacenándolo localmente."
    )]
    [ProducesResponseType(typeof(SupplierContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromForm] SupplierContractRequestDto request, IFormFile? file)
    {
        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos del contrato no son válidos.");

        string? filePath = null;
        if (file != null)
        {
            var isValid = await _localFileStorageService.ValidateFileAsync(file, AllowedFileExtensions, MaxFileSizeInBytes);
            if (!isValid)
                throw new DomainValidationException("Archivo inválido. Formatos permitidos: PDF, DOC, DOCX, JPG, PNG. Tamaño máximo: 10MB.");

            filePath = await _localFileStorageService.SaveFileAsync(file, "contracts");
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
        if (createdContract == null)
            throw new BusinessException("No se pudo crear el contrato del proveedor.");

        var supplier = _getSupplierById.Execute(createdContract.SupplierId);
        if (supplier == null)
            throw new NotFoundException($"Proveedor con ID {createdContract.SupplierId} no encontrado.");

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
                ? await _localFileStorageService.GetFileUrlAsync(createdContract.FilePath)
                : null,
            SupplierId = createdContract.SupplierId,
            SupplierName = supplier.CommercialName,
            CreatedAt = createdContract.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Obtiene un contrato de proveedor por ID.",
        Description = "Devuelve los detalles de un contrato de proveedor específico, incluyendo la URL del archivo asociado."
    )]
    [ProducesResponseType(typeof(SupplierContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            throw new DomainValidationException("Debe proporcionar un ID de contrato válido.");

        var contract = _getSupplierContractById.Execute(id);
        if (contract == null)
            throw new NotFoundException($"Contrato con ID {id} no encontrado.");

        var supplier = _getSupplierById.Execute(contract.SupplierId);
        if (supplier == null)
            throw new NotFoundException($"Proveedor con ID {contract.SupplierId} no encontrado.");

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
                ? await _localFileStorageService.GetFileUrlAsync(contract.FilePath)
                : null,
            SupplierId = contract.SupplierId,
            SupplierName = supplier.CommercialName,
            CreatedAt = contract.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet("supplier/{supplierId}")]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [SwaggerOperation(
        Summary = "Obtiene todos los contratos asociados a un proveedor.",
        Description = "Devuelve la lista completa de contratos asociados al proveedor indicado, incluyendo las URL de archivos."
    )]
    [ProducesResponseType(typeof(IEnumerable<SupplierContractResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBySupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new DomainValidationException("Debe especificar un ID de proveedor válido.");

        var supplier = _getSupplierById.Execute(supplierId);
        if (supplier == null)
            throw new NotFoundException($"Proveedor con ID {supplierId} no encontrado.");

        var contracts = _getContractsBySupplierId.Execute(supplierId);
        if (contracts == null || !contracts.Any())
            throw new NotFoundException("No se encontraron contratos asociados a este proveedor.");

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
                    ? await _localFileStorageService.GetFileUrlAsync(contract.FilePath)
                    : null,
                SupplierId = contract.SupplierId,
                SupplierName = supplier.CommercialName,
                CreatedAt = contract.CreatedAt
            });
        }

        return Ok(response);
    }
}
