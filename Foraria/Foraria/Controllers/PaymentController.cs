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

        // Ping para la validación del endpoint por MercadoPago
        [HttpGet("webhook")]
        public IActionResult Ping()
        {
            _logger.LogInformation("[WEBHOOK] GET recibido (ping de MercadoPago)");
            return Ok("OK");
        }

        // Endpoint para recibir el webhook con notificaciones de Mercado Pago
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
                    return Ok(); // MP requiere un 200 OK siempre
                }

                // Mercado Pago puede mandar "topic" o "type"
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

                return Ok(); // Confirmación de recepción exitosa

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WEBHOOK] Error inesperado");
                return Ok(); // MP requiere un 200 OK siempre, aunque ocurra un error
            }
        }

        private async Task HandlePaymentWebhook(JsonElement json)
        {
            // Intentamos obtener el paymentId desde "data" -> "id"
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
                // Si el paymentId viene en una URL
                var last = paymentIdStr?.Split('/')?.LastOrDefault();
                if (!long.TryParse(last, out paymentId))
                {
                    _logger.LogWarning("[WEBHOOK-PAYMENT] No pude obtener el paymentId");
                    return;
                }
            }

            _logger.LogInformation("[WEBHOOK-PAYMENT] paymentId = {id}", paymentId);

            // Ahora construimos el JsonElement que se espera por ExecuteAsync.
            using var doc = JsonDocument.Parse(json.ToString());
            var body = doc.RootElement;

            // Llamamos al método ExecuteAsync con el JsonElement completo.
            await _processWebHookMP.ExecuteAsync(body);
        }

        private async Task HandleMerchantOrderWebhook(JsonElement json)
        {
            // Extraer el orderId del json
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

            // Crear el JsonElement del body
            using var doc = JsonDocument.Parse(json.ToString());
            var body = doc.RootElement;

            // Llamar a ExecuteAsync con el JsonElement y isMerchantOrder
            await _processWebHookMP.ExecuteAsync(body, isMerchantOrder: true);
        }


    }
}
