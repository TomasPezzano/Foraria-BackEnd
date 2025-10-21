using Foraria.Application.UseCase;
using ForariaDomain.Exceptions;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers
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
