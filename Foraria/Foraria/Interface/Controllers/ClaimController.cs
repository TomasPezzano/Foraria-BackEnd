using System.Threading.Tasks;
using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClaimController : ControllerBase
{

    public readonly ICreateClaim _createClaim;
    public readonly IGetClaims _getClaims;
    public readonly IRejectClaim _rejectClaim;
    public ClaimController(ICreateClaim CreateClaim, IGetClaims GetClaims, IRejectClaim rejectClaim)
    {
        _createClaim = CreateClaim;
        _getClaims = GetClaims;
        _rejectClaim = rejectClaim;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<Claim> claims = await _getClaims.Execute();
        var result = claims.Select(c => new
        {
            claim = new ClaimDto
            {
                Title = c.Title,
                Description = c.Description,
                Priority = c.Priority,
                Category = c.Category,
                Archive = c.Archive,
                User_id = c.User_id,
                CreatedAt = c.CreatedAt
            },
            
            user = c.User != null ? new UserDto
            {
                Id = c.User.Id,
                FirstName = c.User.Name,
                LastName = c.User.LastName,
                Residences = (List<ResidenceDto>)c.User.Residences
            } : null,

            claimResponse = c.ClaimResponse != null ? new ClaimResponseDto
            {
                Description = c.ClaimResponse.Description,
                ResponseDate = c.ClaimResponse.ResponseDate,
                User_id = c.ClaimResponse.User.Id,
                Claim_id = c.ClaimResponse.Claim.Id,
                ResponsibleSector_id = c.ClaimResponse.ResponsibleSector_id
            } : null
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
     public async Task<IActionResult> Add([FromBody] ClaimDto claimDto)
    {

        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? filePath = null;

            // Si viene un archivo en Base64, lo guardamos físicamente
            if (!string.IsNullOrEmpty(claimDto.Archive))
            {
                // Ejemplo: data:image/png;base64,iVBORw0KGgoAAAANS...
                var base64Parts = claimDto.Archive.Split(',');

                if (base64Parts.Length == 2)
                {
                    var base64Data = base64Parts[1];
                    var bytes = Convert.FromBase64String(base64Data);

                    // Detectar tipo de archivo por el prefijo
                    var extension = ".png"; // valor por defecto
                    if (base64Parts[0].Contains("jpeg")) extension = ".jpg";
                    else if (base64Parts[0].Contains("pdf")) extension = ".pdf";
                    else if (base64Parts[0].Contains("mp4")) extension = ".mp4";

                    // Crear carpeta
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "claims");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Nombre único
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Guardar el archivo
                    await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                    // Guardar ruta relativa en BD
                    filePath = $"/uploads/claims/{uniqueFileName}";
                }
            }

            // Crear entidad Claim
            var claim = new Claim
            {
                Title = claimDto.Title,
                Description = claimDto.Description,
                Priority = claimDto.Priority,
                Category = claimDto.Category,
                Archive = filePath,                // opcional
                User_id = claimDto.User_id,        // opcional
                ResidenceId = claimDto.ResidenceId,
                CreatedAt = DateTime.UtcNow,       // obligatorio
                State = "Nuevo"                // obligatorio
            };

            var createdClaim = await _createClaim.Execute(claim);

            var response = new
            {
                createdClaim.Id,
                createdClaim.Title,
                createdClaim.Description,
                createdClaim.Priority,
                createdClaim.Category,
                ArchiveUrl = filePath
            };

            return CreatedAtAction(nameof(GetAll), new { id = createdClaim.Id }, response);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "El formato del archivo Base64 no es válido." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    [HttpPut("reject/{id}")]
    public async Task<IActionResult> RejectClaimById(int id)
    {
        try
        {
            await _rejectClaim.Execute(id);
            return Ok(new { message = $"El reclamo con ID {id} fue rechazado correctamente" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error interno", details = ex.Message });
        }
    }


}
