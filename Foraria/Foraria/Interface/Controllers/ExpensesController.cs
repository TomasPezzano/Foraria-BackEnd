using Foraria.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly GetMonthlyExpenseTotal _getMonthlyExpenseTotal;

        public DashboardController(GetMonthlyExpenseTotal getMonthlyExpenseTotal)
        {
            _getMonthlyExpenseTotal = getMonthlyExpenseTotal;
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
    }
}
