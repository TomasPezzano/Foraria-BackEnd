using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly GetMonthlyExpenseTotal _getMonthlyExpenseTotal;
        private readonly GetExpenseByCategory _getExpenseByCategory;
        private readonly GetPendingExpenses _getPendingExpenses;
        private readonly GetUserExpenseSummary _getUserExpenseSummary;
        private readonly GetUserMonthlyExpenseHistory _getUserMonthlyExpenseHistory;
        private readonly GetActivePollCount _getActivePollCount;
        private readonly GetUpcomingReserves _getUpcomingReserves;
        private readonly GetActiveReserveCount _getActiveReserveCount;

        public DashboardController(
            GetMonthlyExpenseTotal getMonthlyExpenseTotal,
            GetExpenseByCategory getExpenseByCategory,
            GetPendingExpenses getPendingExpenses,
            GetUserExpenseSummary getUserExpenseSummary,
            GetUserMonthlyExpenseHistory getUserMonthlyExpenseHistory,
            GetActivePollCount getActivePollCount,
            GetUpcomingReserves getUpcomingReserves,
            GetActiveReserveCount getActiveReserveCount)
        {
            _getMonthlyExpenseTotal = getMonthlyExpenseTotal;
            _getExpenseByCategory = getExpenseByCategory;
            _getPendingExpenses = getPendingExpenses;
            _getUserExpenseSummary = getUserExpenseSummary;
            _getUserMonthlyExpenseHistory = getUserMonthlyExpenseHistory;
            _getActivePollCount = getActivePollCount;
            _getUpcomingReserves = getUpcomingReserves;
            _getActiveReserveCount = getActiveReserveCount;
        }

        [HttpGet("expenses/total")]
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

        [HttpGet("expenses/category")]
        public async Task<IActionResult> GetExpenseByCategory(
            [FromQuery] int consortiumId,
            [FromQuery] DateTime? date = null)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe proporcionar un ID de consorcio válido.");

            var result = await _getExpenseByCategory.ExecuteAsync(consortiumId, date);

            if (result == null || !result.Any())
                throw new NotFoundException("No se encontraron gastos para las categorías especificadas.");

            return Ok(new
            {
                consortiumId,
                month = (date ?? DateTime.UtcNow).ToString("MMMM yyyy"),
                data = result
            });
        }

        [HttpGet("expenses/pending")]
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
