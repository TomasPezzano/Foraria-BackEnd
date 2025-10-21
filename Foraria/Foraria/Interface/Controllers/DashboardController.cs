using Foraria.Application.UseCase;
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

        public DashboardController(
            GetMonthlyExpenseTotal getMonthlyExpenseTotal,
            GetExpenseByCategory getExpenseByCategory,
            GetPendingExpenses getPendingExpenses,
            GetUserExpenseSummary getUserExpenseSummary,
            GetUserMonthlyExpenseHistory getUserMonthlyExpenseHistory)
        {
            _getMonthlyExpenseTotal = getMonthlyExpenseTotal;
            _getExpenseByCategory = getExpenseByCategory;
            _getPendingExpenses = getPendingExpenses;
            _getUserExpenseSummary = getUserExpenseSummary;
            _getUserMonthlyExpenseHistory = getUserMonthlyExpenseHistory;
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
            if (userId <= 0)
                throw new ValidationException("Debe proporcionar un ID de usuario válido.");

            var result = await _getUserMonthlyExpenseHistory.ExecuteAsync(userId, year);

            if (result == null)
                throw new NotFoundException("No se encontró historial mensual de gastos para este usuario.");

            if (result is IEnumerable<object> collection && !collection.Any())
                throw new NotFoundException("No se encontró historial mensual de gastos para este usuario.");

            return Ok(result);
        }
    }
}
