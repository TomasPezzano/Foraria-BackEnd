using Foraria.Application.UseCase;
using Foraria.Interface.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimResponseController : ControllerBase
    {

        public readonly CreateClaimResponse CreateClaimResponse;
        public ClaimResponseController(CreateClaimResponse CreateClaimResponse)
        {
            this.CreateClaimResponse = CreateClaimResponse;
        }

        [HttpPost]
        public IActionResult Add([FromBody] ClaimResponseDto claimResponseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            CreateClaimResponse.Execute(claimResponseDto);

            return Ok("Respuesta desde ClaimResponseController");
        }
    }
}
