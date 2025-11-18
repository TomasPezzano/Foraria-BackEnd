using Foraria.Application.Services;
using foraria.application.usecase;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers
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
        private readonly IPermissionService _permissionService;

        public DashboardAdminController(
            GetTotalUsers getTotalUsers,
            GetPendingClaimsCount getPendingClaimsCount,
            GetLatestPendingClaim getLatestPendingClaim,
            GetCollectedExpensesPercentage getCollectedExpensesPercentage,
            GetUpcomingReserves getUpcomingReserves,
            IPermissionService permissionService)
        {
            _getTotalUsers = getTotalUsers;
            _getPendingClaimsCount = getPendingClaimsCount;
            _getLatestPendingClaim = getLatestPendingClaim;
            _getCollectedExpensesPercentage = getCollectedExpensesPercentage;
            _getUpcomingReserves = getUpcomingReserves;
            _permissionService = permissionService;
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
            await _permissionService.EnsurePermissionAsync(User, "Dashboard.ViewUsersCount");

            if (consortiumId is < 0)
                throw new DomainValidationException("El ID del consorcio no puede ser negativo.");

            var count = await _getTotalUsers.ExecuteAsync();
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
            await _permissionService.EnsurePermissionAsync(User, "Dashboard.ViewPendingClaims");

            if (consortiumId is < 0)
                throw new DomainValidationException("El ID del consorcio no puede ser negativo.");

            var count = await _getPendingClaimsCount.ExecuteAsync();
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
            await _permissionService.EnsurePermissionAsync(User, "Dashboard.ViewLatestClaim");

            var claim = await _getLatestPendingClaim.ExecuteAsync();

            if (claim == null)
                throw new NotFoundException("No hay reclamos pendientes para este consorcio.");

            return Ok(claim);
        }
        
        [HttpGet("expenses/collected-percentage")]
        //[Authorize(Policy = "ConsortiumAndAdmin")]
        [SwaggerOperation(
            Summary = "Obtiene el porcentaje de expensas recaudadas.",
            Description = "Calcula el porcentaje total de expensas cobradas frente al total emitido, para el consorcio indicado y la fecha (opcional)."
        )]
        public async Task<IActionResult> GetCollectedExpensesPercentage(
            [FromQuery] int consortiumId,
            [FromQuery] DateTime? date = null)
        {
            if (consortiumId <= 0)
                throw new DomainValidationException("Debe especificar un ID de consorcio válido.");

            var result = await _getCollectedExpensesPercentage.ExecuteAsync(date);
            return Ok(result);
        }
       
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
            await _permissionService.EnsurePermissionAsync(User, "Dashboard.ViewUpcomingReservations");

            if (limit <= 0)
                throw new DomainValidationException("El límite debe ser mayor a cero.");

            var result = await _getUpcomingReserves.ExecuteAsync(limit);

            if (result is IEnumerable<object> collection && !collection.Any())
                throw new NotFoundException("No se encontraron reservas próximas.");

            if (result == null)
                throw new NotFoundException("No se encontraron reservas próximas.");

            return Ok(result);
        }
    }
}
