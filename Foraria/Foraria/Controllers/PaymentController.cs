using Foraria.Application.Services;
using Foraria.Infrastructure.Persistence;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly CreatePreferenceMP _createPreferenceMP;
        private readonly ProcessWebHookMP _processWebHookMP;
        private readonly ForariaContext _context;
        private readonly IPermissionService _permissionService;

        public PaymentController(
            CreatePreferenceMP createPreferenceMP,
            ForariaContext context,
            ProcessWebHookMP processWebHookMP,
            IPermissionService permissionService)
        {
            _createPreferenceMP = createPreferenceMP;
            _context = context;
            _processWebHookMP = processWebHookMP;
            _permissionService = permissionService;
        }


        [HttpPost("create-preference")]
        [SwaggerOperation(
            Summary = "Crea una preferencia de pago en Mercado Pago.",
            Description = "Genera una preferencia de pago asociada a una expensa y una residencia, retornando el enlace de pago y detalles del proceso."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePreference(int expenseId, int residenceId)
        {
            await _permissionService.EnsurePermissionAsync(User, "Payments.CreatePreference");

            if (expenseId <= 0)
                throw new DomainValidationException("Debe especificar un ID de expensa válido.");

            if (residenceId <= 0)
                throw new DomainValidationException("Debe especificar un ID de residencia válido.");

            var result = await _createPreferenceMP.ExecuteAsync(expenseId, residenceId);

            if (result == null)
                throw new BusinessException("No se pudo generar la preferencia de pago.");

            return Ok(result);
        }

        [HttpPost("webhook")]
        [SwaggerOperation(
            Summary = "Procesa el webhook de Mercado Pago.",
            Description = "Recibe las notificaciones enviadas por Mercado Pago sobre el estado de las transacciones y las procesa de forma asincrónica."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body)
        {
            if (body.ValueKind == JsonValueKind.Undefined || body.ValueKind == JsonValueKind.Null)
                throw new DomainValidationException("El cuerpo del webhook está vacío o no es válido.");

            await _processWebHookMP.ExecuteAsync(body);

            return Ok();
        }
    }
}
