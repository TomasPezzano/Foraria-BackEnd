using Foraria.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly Calculator _calculator = new();

        [HttpGet("sum")]
        public ActionResult<int> GetSum(int a, int b)
        {
            return _calculator.Sum(a, b);
        }
    }
}
