using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Foraria
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no manejada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                BusinessException => (int)HttpStatusCode.Conflict,
                NotFoundException or ThreadNotFoundException => (int)HttpStatusCode.NotFound,
                ThreadLockedException => (int)HttpStatusCode.Conflict,
                ThreadOwnershipException or ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
                ThreadUpdateException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };


            var result = JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                type = ex.GetType().Name,
                statusCode
            });

            response.StatusCode = statusCode;
            await response.WriteAsync(result);
        }
    }
}
