using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest("Debe subir un archivo válido.");

            var tempPath = Path.GetTempFileName();
            await using (var stream = System.IO.File.Create(tempPath))
            {
                await request.File.CopyToAsync(stream);
            }

            try
            {
                var documentId = Guid.NewGuid();

                var proof = await _notarizeFile.ExecuteAsync(documentId, tempPath);

                System.IO.File.Delete(tempPath);

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
            catch (Exception ex)
            {
                System.IO.File.Delete(tempPath);
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpPost("verify-file")]
        public async Task<IActionResult> VerifyFile([FromForm] VerifyFileRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Debe subir un archivo válido.");

            var tempPath = Path.GetTempFileName();
            await using (var stream = System.IO.File.Create(tempPath))
            {
                await request.File.CopyToAsync(stream);
            }

            try
            {
                var isValid = await _verifyFileProof.ExecuteAsync(request.DocumentId, tempPath);
                System.IO.File.Delete(tempPath);

                return Ok(new
                {
                    documentId = request.DocumentId,
                    valid = isValid,
                    message = isValid
                        ? "El archivo coincide con la prueba registrada en blockchain."
                        : "El archivo fue modificado o no fue notarizado."
                });
            }
            catch (Exception ex)
            {
                System.IO.File.Delete(tempPath);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
