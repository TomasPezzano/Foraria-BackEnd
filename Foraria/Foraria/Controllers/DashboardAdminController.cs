using Foraria.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Controllers
{
    [ApiController]
    [Route("api/dashboard/admin")]
    public class DashboardAdminController : ControllerBase
    {
        private readonly GetTotalUsers _getTotalUsers;
        private readonly GetPendingClaimsCount _getPendingClaimsCount;
        private readonly GetLatestPendingClaim _getLatestPendingClaim;
        //private readonly GetCollectedExpensesPercentage _getCollectedExpensesPercentage;
        private readonly GetUpcomingReserves _getUpcomingReserves;

        public DashboardAdminController(
            GetTotalUsers getTotalUsers,
            GetPendingClaimsCount getPendingClaimsCount,
            GetLatestPendingClaim getLatestPendingClaim,
            //GetCollectedExpensesPercentage getCollectedExpensesPercentage,
            GetUpcomingReserves getUpcomingReserves)
        {
            _getTotalUsers = getTotalUsers;
            _getPendingClaimsCount = getPendingClaimsCount;
            _getLatestPendingClaim = getLatestPendingClaim;
            //_getCollectedExpensesPercentage = getCollectedExpensesPercentage;
            _getUpcomingReserves = getUpcomingReserves;
        }

        [HttpGet("users/count")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene el total de usuarios registrados.",
            Description = "Devuelve la cantidad total de usuarios del sistema o, si se indica un consorcio, el número de usuarios asociados a dicho consorcio."
        )]
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
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene la cantidad de reclamos pendientes.",
            Description = "Devuelve el número total de reclamos sin resolver en el sistema o en un consorcio específico."
        )]
        public async Task<IActionResult> GetPendingClaimsCount([FromQuery] int? consortiumId = null)
        {
            if (consortiumId is < 0)
                throw new ValidationException("El ID del consorcio no puede ser negativo.");

            var count = await _getPendingClaimsCount.ExecuteAsync(consortiumId);
            return Ok(new { consortiumId, pendingClaims = count });
        }

        [HttpGet("claims/latest")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene el reclamo pendiente más reciente.",
            Description = "Devuelve la información del último reclamo pendiente registrado en el sistema o en el consorcio indicado."
        )]
        public async Task<IActionResult> GetLatestPendingClaim([FromQuery] int? consortiumId = null)
        {
            var claim = await _getLatestPendingClaim.ExecuteAsync(consortiumId);

            if (claim == null)
                throw new NotFoundException("No hay reclamos pendientes para este consorcio.");

            return Ok(claim);
        }
        /*
        [HttpGet("expenses/collected-percentage")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene el porcentaje de expensas recaudadas.",
            Description = "Calcula el porcentaje total de expensas cobradas frente al total emitido, para el consorcio indicado y la fecha (opcional)."
        )]
        public async Task<IActionResult> GetCollectedExpensesPercentage(
            [FromQuery] int consortiumId,
            [FromQuery] DateTime? date = null)
        {
            if (consortiumId <= 0)
                throw new ValidationException("Debe especificar un ID de consorcio válido.");

            var result = await _getCollectedExpensesPercentage.ExecuteAsync(consortiumId, date);
            return Ok(result);
        }
        */
        [HttpGet("reservations/upcoming")]
        [Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene las próximas reservas programadas.",
            Description = "Devuelve un listado con las próximas reservas activas dentro del consorcio, ordenadas por fecha."
        )]
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
