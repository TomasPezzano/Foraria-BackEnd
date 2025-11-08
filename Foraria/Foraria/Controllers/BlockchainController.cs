using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.DTOs;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly NotarizeFile _notarizeFile;
        private readonly VerifyFileProof _verifyFileProof;

        public BlockchainController(
            IBlockchainService blockchainService,
            IBlockchainProofRepository proofRepository)
        {
            _notarizeFile = new NotarizeFile(blockchainService, proofRepository);
            _verifyFileProof = new VerifyFileProof(blockchainService, proofRepository);
        }

        [HttpPost("notarize-file")]
        [SwaggerOperation(
            Summary = "Notariza un archivo en la blockchain.",
            Description = "Registra el hash del archivo en la blockchain junto con metadatos de prueba."
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> NotarizeFile([FromForm] NotarizeFileRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ValidationException("Debe subir un archivo válido.");

            var tempPath = Path.GetTempFileName();

            try
            {
                await using (var stream = System.IO.File.Create(tempPath))
                    await request.File.CopyToAsync(stream);

                var documentId = Guid.NewGuid();

                var proof = await _notarizeFile.ExecuteAsync(documentId, tempPath)
                    ?? throw new BlockchainException("Error al generar la prueba en blockchain.");

                return Ok(new
                {
                    message = "Archivo notarizado correctamente.",
                    proof.Id,
                    proof.DocumentId,
                    proof.HashHex,
                    proof.TxHash,
                    proof.Uri,
                    proof.Network,
                    proof.ChainId,
                    proof.CreatedAtUtc
                });
            }
            catch (BlockchainException)
            {
                throw;
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        [HttpPost("verify-file")]
        [SwaggerOperation(
            Summary = "Verifica un archivo contra su prueba registrada en blockchain.",
            Description = "Comprueba si el archivo coincide con la prueba registrada."
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyFile([FromForm] VerifyFileRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ValidationException("Debe subir un archivo válido.");

            var tempPath = Path.GetTempFileName();

            try
            {
                await using (var stream = System.IO.File.Create(tempPath))
                    await request.File.CopyToAsync(stream);

                var isValid = await _verifyFileProof.ExecuteAsync(request.DocumentId, tempPath);

                return Ok(new
                {
                    documentId = request.DocumentId,
                    valid = isValid,
                    message = isValid
                        ? "El archivo coincide con la prueba registrada en blockchain."
                        : "El archivo fue modificado o no fue notarizado."
                });
            }
            catch (NotFoundException)
            {
                throw new BlockchainException("No se encontró la prueba de blockchain asociada al documento.");
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }
    }
}
