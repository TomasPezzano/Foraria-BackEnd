using ForariaDomain.Exceptions;
using ForariaDomain.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Foraria.Infrastructure.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool HasActiveTenant()
    {
        return GetCurrentConsortiumIdOrNull().HasValue;
    }

    public int? GetCurrentConsortiumIdOrNull()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
            return null;

        if (httpContext.User?.Identity?.IsAuthenticated != true)
            return null;

        if (httpContext.Request.Headers.TryGetValue("X-Consortium-Id", out var consortiumIdHeader))
        {
            if (int.TryParse(consortiumIdHeader.ToString(), out int consortiumId))
            {
                if (!HasAccessToConsortium(consortiumId))
                    throw new ForbiddenAccessException($"No tienes acceso al consorcio {consortiumId}.");

                return consortiumId;
            }
        }


        var userConsortiums = GetUserConsortiumIds();

        if (userConsortiums.Count == 0)
            return null;

        if (userConsortiums.Count == 1)
            return userConsortiums[0];

        throw new BusinessException(
            "Tienes acceso a múltiples consorcios. Debes especificar cuál usar mediante el header 'X-Consortium-Id'.");
    }

    public int GetCurrentConsortiumId()
    {
        var consortiumId = GetCurrentConsortiumIdOrNull();

        if (!consortiumId.HasValue)
            throw new BusinessException(
                "Esta operación requiere un consorcio activo. Especifica uno mediante el header 'X-Consortium-Id'.");

        return consortiumId.Value;
    }

    public int GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException("Usuario no autenticado.");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Token inválido: no se pudo obtener el ID de usuario.");

        return userId;
    }

    public bool HasAccessToConsortium(int consortiumId)
    {
        var userConsortiums = GetUserConsortiumIds();
        return userConsortiums.Contains(consortiumId);
    }

    public List<int> GetUserConsortiumIds()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
            return new List<int>();

        var consortiumClaims = user.FindAll("consortiumId")
            .Select(c => int.TryParse(c.Value, out int id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        return consortiumClaims;
    }
}