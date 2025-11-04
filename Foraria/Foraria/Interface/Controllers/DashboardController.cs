using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly GetMonthlyExpenseTotal _getMonthlyExpenseTotal;
        private readonly GetPendingExpenses _getPendingExpenses;
        private readonly GetUserExpenseSummary _getUserExpenseSummary;
        private readonly GetUserMonthlyExpenseHistory _getUserMonthlyExpenseHistory;
        private readonly GetActivePollCount _getActivePollCount;
        private readonly GetUpcomingReserves _getUpcomingReserves;
        private readonly GetActiveReserveCount _getActiveReserveCount;

        public DashboardController(
            GetMonthlyExpenseTotal getMonthlyExpenseTotal,
            GetPendingExpenses getPendingExpenses,
            GetUserExpenseSummary getUserExpenseSummary,
            GetUserMonthlyExpenseHistory getUserMonthlyExpenseHistory,
            GetActivePollCount getActivePollCount,
            GetUpcomingReserves getUpcomingReserves,
            GetActiveReserveCount getActiveReserveCount)
        {
            _getMonthlyExpenseTotal = getMonthlyExpenseTotal;
            _getPendingExpenses = getPendingExpenses;
            _getUserExpenseSummary = getUserExpenseSummary;
            _getUserMonthlyExpenseHistory = getUserMonthlyExpenseHistory;
            _getActivePollCount = getActivePollCount;
            _getUpcomingReserves = getUpcomingReserves;
            _getActiveReserveCount = getActiveReserveCount;
        }
        
        [HttpGet("expenses/total")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene el total mensual de gastos.",
            Description = "Devuelve el monto total de expensas del mes actual para un consorcio específico."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMonthlyExpenseTotal([FromQuery] int consortiumId)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var total = await _getMonthlyExpenseTotal.ExecuteAsync(consortiumId);

            if (total < 0)
                throw new BusinessException("El total mensual de gastos no puede ser negativo.");

            return Ok(new
            {
                consortiumId,
                totalAmount = total,
                month = DateTime.UtcNow.ToString("MMMM yyyy")
            });
        }

       
        
        [HttpGet("expenses/pending")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene las expensas pendientes.",
            Description = "Lista todas las expensas aún no pagadas correspondientes al consorcio indicado."
        )]
        public async Task<IActionResult> GetPendingExpenses([FromQuery] int consortiumId)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var result = await _getPendingExpenses.ExecuteAsync(consortiumId);

            if (result == null || !result.Any())
                throw new NotFoundException("No hay gastos pendientes para este consorcio.");

            return Ok(new
            {
                consortiumId,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                pendingExpenses = result
            });
        }
       
        
        [HttpGet("expenses/summary")]
        [Authorize(Policy = "OwnerAndTenant")]
        [SwaggerOperation(
            Summary = "Obtiene el resumen de gastos del usuario.",
            Description = "Devuelve el estado de pagos del usuario, incluyendo expensas pagadas, vencidas y próximas a vencer."
        )]
        public async Task<IActionResult> GetUserExpenseSummary([FromQuery] int userId)
        {
            if (userId <= 0)
                throw new ValidationException("Debe proporcionar un ID de usuario válido.");

            var result = await _getUserExpenseSummary.ExecuteAsync(userId);

            if (result == null)
                throw new NotFoundException("No se encontró resumen de gastos para este usuario.");

            return Ok(new
            {
                userId,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                data = result
            });
        }

        [HttpGet("expenses/monthly-history")]
        [Authorize(Policy = "OwnerAndTenant")]
        [SwaggerOperation(
            Summary = "Obtiene el historial mensual de gastos del usuario.",
            Description = "Devuelve la evolución de los gastos del usuario a lo largo del año actual o el año especificado."
        )]
        public async Task<IActionResult> GetUserMonthlyExpenseHistory(
            [FromQuery] int userId,
            [FromQuery] int? year = null)
        {
            if (userId < 0)
                throw new ValidationException("Debe proporcionar un ID de usuario válido.");

            var result = await _getUserMonthlyExpenseHistory.ExecuteAsync(userId, year);

            if (result == null)
                throw new NotFoundException("No se encontró historial mensual de gastos para este usuario.");

            if (result is IEnumerable<object> collection && !collection.Any())
                throw new NotFoundException("No se encontró historial mensual de gastos para este usuario.");

            return Ok(result);
        }

        [HttpGet("polls/active")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene la cantidad de votaciones activas.",
            Description = "Devuelve la cantidad de votaciones abiertas actualmente en el consorcio."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivePolls([FromQuery] int consortiumId)
        {
            if (consortiumId < 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var count = await _getActivePollCount.ExecuteAsync(consortiumId);

            if (count < 0)
                throw new BusinessException("El número de votaciones activas no puede ser negativo.");

            if (count == 0)
                throw new NotFoundException("No hay votaciones activas actualmente para este consorcio.");

            return Ok(new
            {
                consortiumId,
                activePolls = count,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        [HttpGet("reservations/active")]
        [Authorize(Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene las reservas activas.",
            Description = "Devuelve un listado de las próximas reservas activas para el consorcio indicado, ordenadas por fecha."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveReservations([FromQuery] int consortiumId, [FromQuery] int limit = 5)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var result = await _getUpcomingReserves.ExecuteAsync(consortiumId, limit);

            var reservationsProperty = result.GetType().GetProperty("upcomingReservations");
            var upcomingReservations = reservationsProperty?.GetValue(result) as IEnumerable<object>;

            if (upcomingReservations == null || !upcomingReservations.Any())
                throw new NotFoundException("No hay reservas activas para este consorcio.");

            return Ok(result);
        }

        [HttpGet("reservations/count")]
        [Authorize (Policy = "All")]
        [SwaggerOperation(
            Summary = "Obtiene la cantidad de reservas activas.",
            Description = "Devuelve el número total de reservas activas o próximas dentro del consorcio especificado."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveReservationsCount([FromQuery] int consortiumId)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var count = await _getActiveReserveCount.ExecuteAsync(consortiumId);

            if (count < 0)
                throw new BusinessException("El número de reservas activas no puede ser negativo.");

            if (count == 0)
                throw new NotFoundException("No hay reservas activas actualmente para este consorcio.");

            return Ok(new
            {
                consortiumId,
                activeReservations = count,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
