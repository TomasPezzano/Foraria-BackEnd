using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Foraria.DTOs;

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
            Description = "Recibe un archivo, genera su hash SHA-256 y lo registra en la blockchain (actualmente en Polygon) junto con un identificador de documento único. Devuelve los datos de la prueba creada, incluyendo el hash, el hash de transacción y la red utilizada."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                var proof = await _notarizeFile.ExecuteAsync(documentId, tempPath);

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
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        [HttpPost("verify-file")]
        [SwaggerOperation(
            Summary = "Verifica un archivo contra su prueba registrada en blockchain.",
            Description = "Recibe un archivo y un ID de documento, calcula su hash y verifica si coincide con la prueba registrada previamente en la blockchain. Devuelve si la verificación es válida o si el archivo fue alterado."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }
    }
}
