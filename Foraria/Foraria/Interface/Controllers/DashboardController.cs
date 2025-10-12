using Foraria.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

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

        public DashboardController(GetMonthlyExpenseTotal getMonthlyExpenseTotal, GetExpenseByCategory getExpenseByCategory, GetPendingExpenses getPendingExpenses, GetUserExpenseSummary getUserExpenseSummary, GetUserMonthlyExpenseHistory getUserMonthlyExpenseHistory)
        {
            _getMonthlyExpenseTotal = getMonthlyExpenseTotal;
            _getExpenseByCategory = getExpenseByCategory;
            _getPendingExpenses = getPendingExpenses;
            _getUserExpenseSummary = getUserExpenseSummary;
            _getUserMonthlyExpenseHistory = getUserMonthlyExpenseHistory;
        }

        [HttpGet("expenses/total")]
        public async Task<IActionResult> GetMonthlyExpenseTotal([FromQuery] int consortiumId)
        {
            var total = await _getMonthlyExpenseTotal.ExecuteAsync(consortiumId);
            return Ok(new
            {
                totalAmount = total,
                month = DateTime.UtcNow.ToString("MMMM yyyy")
            });
        }

        [HttpGet("expenses/category")]
        public async Task<IActionResult> GetExpenseByCategory(
        [FromQuery] int consortiumId,
        [FromQuery] DateTime? date = null)
        {
            var result = await _getExpenseByCategory.ExecuteAsync(consortiumId, date);
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
            var result = await _getPendingExpenses.ExecuteAsync(consortiumId);
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
            var result = await _getUserExpenseSummary.ExecuteAsync(userId);
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
            var result = await _getUserMonthlyExpenseHistory.ExecuteAsync(userId, year);
            return Ok(result);
        }
    }
}
