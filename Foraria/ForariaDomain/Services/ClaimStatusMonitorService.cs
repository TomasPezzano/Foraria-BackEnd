using Foraria.Domain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Services;

public class ClaimStatusMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClaimStatusMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1); 

    public ClaimStatusMonitorService(
        IServiceProvider serviceProvider,
        ILogger<ClaimStatusMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClaimStatusMonitorService iniciado");

        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOldPendingClaimsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ClaimStatusMonitorService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("ClaimStatusMonitorService detenido");
    }

    private async Task CheckOldPendingClaimsAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var claimRepository = scope.ServiceProvider.GetRequiredService<IClaimRepository>();

        try
        {
            var allClaims = await claimRepository.GetAll();

            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            var oldPendingClaims = allClaims
                .Where(c =>
                    c.State.ToLower() == "pendiente" ||
                    c.State.ToLower() == "pending")
                .Where(c => c.CreatedAt < sevenDaysAgo)
                .ToList();

            _logger.LogInformation(
                "Encontrados {Count} reclamos pendientes por más de 7 días",
                oldPendingClaims.Count);

            foreach (var claim in oldPendingClaims)
            {
                _logger.LogWarning(
                    "Reclamo {ClaimId} - '{Title}' lleva {Days} días pendiente",
                    claim.Id,
                    claim.Title,
                    (DateTime.Now - claim.CreatedAt).Days);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar reclamos antiguos");
        }
    }
}
