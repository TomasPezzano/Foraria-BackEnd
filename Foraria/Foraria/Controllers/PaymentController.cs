using Foraria.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ForariaDomain.Application.UseCase;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly CreatePreferenceMP _createPreferenceMP;
        private readonly ProcessWebHookMP _processWebHookMP;
        private readonly ForariaContext _context;

        public PaymentController(CreatePreferenceMP createPreferenceMP, ForariaContext context, ProcessWebHookMP processWebHookMP)
        {
            _createPreferenceMP = createPreferenceMP;
            _context = context;
            _processWebHookMP = processWebHookMP;
        }

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference(int expenseId, int residenceId)
        {
            var result = await _createPreferenceMP.ExecuteAsync(expenseId, residenceId);
            return Ok(result);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body)
        {
            try
            {
                Console.WriteLine("🟡 Webhook recibido.");
                await _processWebHookMP.ExecuteAsync(body);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando webhook: {ex.Message}");
                return Ok(); 
            }
        }
    }
}
