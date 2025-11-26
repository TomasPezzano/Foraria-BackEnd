using Foraria.Application.Services;
using Foraria.Infrastructure.Persistence;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Foraria.DTOs;

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
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            CreatePreferenceMP createPreferenceMP,
            ForariaContext context,
            ProcessWebHookMP processWebHookMP,
            IPermissionService permissionService,
            ILogger<PaymentController> logger)
        {
            _createPreferenceMP = createPreferenceMP;
            _context = context;
            _processWebHookMP = processWebHookMP;
            _permissionService = permissionService;
            _logger = logger;
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

        [HttpGet("webhook")]
        public IActionResult Ping()
        {
            _logger.LogInformation("[WEBHOOK] GET recibido (ping de MercadoPago)");
            return Ok("OK");
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body)
        {
            try
            {
                _logger.LogInformation("[WEBHOOK] POST recibido: {json}", body.ToString());

                if (body.ValueKind == JsonValueKind.Undefined ||
                    body.ValueKind == JsonValueKind.Null)
                {
                    _logger.LogWarning("[WEBHOOK] Body vacío o inválido");
                    return Ok(); 
                }

                string topic = body.GetPropertyOrDefault("topic")
                               ?? body.GetPropertyOrDefault("type");

                if (string.IsNullOrEmpty(topic))
                {
                    _logger.LogWarning("[WEBHOOK] Sin topic/type");
                    return Ok();
                }

                switch (topic)
                {
                    case "payment":
                        await HandlePaymentWebhook(body);
                        break;

                    case "merchant_order":
                        await HandleMerchantOrderWebhook(body);
                        break;

                    default:
                        _logger.LogWarning("[WEBHOOK] Topic no manejado: {topic}", topic);
                        break;
                }

                return Ok(); 

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WEBHOOK] Error inesperado");
                return Ok(); 
            }
        }

        private async Task HandlePaymentWebhook(JsonElement json)
        {
            string? paymentIdStr = json.GetNested("data", "id")
                                   ?? json.GetPropertyOrDefault("id")
                                   ?? json.GetPropertyOrDefault("resource");

            if (string.IsNullOrEmpty(paymentIdStr))
            {
                _logger.LogWarning("[WEBHOOK-PAYMENT] No pude obtener el paymentId.");
                return;
            }

            if (!long.TryParse(paymentIdStr, out long paymentId))
            {
                var last = paymentIdStr?.Split('/')?.LastOrDefault();
                if (!long.TryParse(last, out paymentId))
                {
                    _logger.LogWarning("[WEBHOOK-PAYMENT] No pude obtener el paymentId");
                    return;
                }
            }

            _logger.LogInformation("[WEBHOOK-PAYMENT] paymentId = {id}", paymentId);

            using var doc = JsonDocument.Parse(json.ToString());
            var body = doc.RootElement;

            await _processWebHookMP.ExecuteAsync(body);
        }

        private async Task HandleMerchantOrderWebhook(JsonElement json)
        {
            string? orderIdStr = json.GetNested("data", "id")
                                 ?? json.GetPropertyOrDefault("id");

            if (string.IsNullOrEmpty(orderIdStr))
            {
                _logger.LogWarning("[WEBHOOK-ORDER] No pude obtener orderId.");
                return;
            }

            if (!long.TryParse(orderIdStr, out long orderId))
            {
                var last = orderIdStr?.Split('/')?.LastOrDefault();
                if (!long.TryParse(last, out orderId))
                {
                    _logger.LogWarning("[WEBHOOK-ORDER] No pude obtener orderId");
                    return;
                }
            }

            _logger.LogInformation("[WEBHOOK-ORDER] orderId = {id}", orderId);

            using var doc = JsonDocument.Parse(json.ToString());
            var body = doc.RootElement;

            await _processWebHookMP.ExecuteAsync(body, isMerchantOrder: true);
        }


    }
}
