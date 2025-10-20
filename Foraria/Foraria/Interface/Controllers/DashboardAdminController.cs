using Foraria.Application.UseCase;
using ForariaDomain.Exceptions;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers
{
    [ApiController]
    [Route("api/dashboard/admin")]
    public class DashboardAdminController : ControllerBase
    {
        private readonly GetTotalUsers _getTotalUsers;
        private readonly GetPendingClaimsCount _getPendingClaimsCount;
        private readonly GetLatestPendingClaim _getLatestPendingClaim;
        private readonly GetCollectedExpensesPercentage _getCollectedExpensesPercentage;
        private readonly GetUpcomingReserves _getUpcomingReserves;

        public DashboardAdminController(
            GetTotalUsers getTotalUsers,
            GetPendingClaimsCount getPendingClaimsCount,
            GetLatestPendingClaim getLatestPendingClaim,
            GetCollectedExpensesPercentage getCollectedExpensesPercentage,
            GetUpcomingReserves getUpcomingReserves)
        {
            _getTotalUsers = getTotalUsers;
            _getPendingClaimsCount = getPendingClaimsCount;
            _getLatestPendingClaim = getLatestPendingClaim;
            _getCollectedExpensesPercentage = getCollectedExpensesPercentage;
            _getUpcomingReserves = getUpcomingReserves;
        }

        [HttpGet("users/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTotalUsers([FromQuery] int? consortiumId = null)
        {
            if (consortiumId is < 0)
                throw new ValidationException("El ID del consorcio no puede ser negativo.");

            var count = await _getTotalUsers.ExecuteAsync(consortiumId);
            return Ok(new { consortiumId, totalUsers = count });
        }

        [HttpGet("claims/pending-count")]
        public async Task<IActionResult> GetPendingClaimsCount([FromQuery] int? consortiumId = null)
        {
            if (consortiumId is < 0)
                throw new ValidationException("El ID del consorcio no puede ser negativo.");

            var count = await _getPendingClaimsCount.ExecuteAsync(consortiumId);
            return Ok(new { consortiumId, pendingClaims = count });
        }

        [HttpGet("claims/latest")]
        public async Task<IActionResult> GetLatestPendingClaim([FromQuery] int? consortiumId = null)
        {
            var claim = await _getLatestPendingClaim.ExecuteAsync(consortiumId);

            if (claim == null)
                throw new NotFoundException("No hay reclamos pendientes para este consorcio.");

            return Ok(claim);
        }

        [HttpGet("expenses/collected-percentage")]
        public async Task<IActionResult> GetCollectedExpensesPercentage(
            [FromQuery] int consortiumId,
            [FromQuery] DateTime? date = null)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe especificar un ID de consorcio válido.");

            var result = await _getCollectedExpensesPercentage.ExecuteAsync(consortiumId, date);
            return Ok(result);
        }

        [HttpGet("reservations/upcoming")]
        public async Task<IActionResult> GetUpcomingReservations(
            [FromQuery] int consortiumId,
            [FromQuery] int limit = 5)
        {
            if (limit <= 0)
                throw new ValidationException("El límite debe ser mayor a cero.");

            var result = await _getUpcomingReserves.ExecuteAsync(consortiumId, limit);

            if (result is IEnumerable<object> collection && !collection.Any())
                throw new NotFoundException("No se encontraron reservas próximas.");

            if (result == null)
                throw new NotFoundException("No se encontraron reservas próximas.");

            return Ok(result);
        }
    }
}
