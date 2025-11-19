using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Services;

public class PollNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PollNotificationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public PollNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PollNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PollNotificationBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckClosingSoonPollsAsync();
                await CheckClosedPollsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PollNotificationBackgroundService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckClosingSoonPollsAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var pollRepository = scope.ServiceProvider.GetRequiredService<IPollRepository>();
        var sendPollNotification = scope.ServiceProvider.GetRequiredService<ISendPollNotification>();

        try
        {
            var tomorrow = DateTime.Now.AddDays(1);
            var today = DateTime.Now;

            var closingSoonPolls = await pollRepository.GetClosingSoonAsync(today, tomorrow);

            _logger.LogInformation(
                "Encontradas {Count} votaciones por cerrar",
                closingSoonPolls.Count());

            foreach (var poll in closingSoonPolls)
            {
                try
                {
                    await sendPollNotification.ExecuteForClosingSoonAsync(poll.Id);

                    _logger.LogInformation(
                        "Recordatorio enviado para votación {PollId}: {Title}",
                        poll.Id, poll.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al enviar recordatorio para votación {PollId}",
                        poll.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar votaciones por cerrar");
        }
    }

    private async Task CheckClosedPollsAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var pollRepository = scope.ServiceProvider.GetRequiredService<IPollRepository>();
        var sendPollNotification = scope.ServiceProvider.GetRequiredService<ISendPollNotification>();

        try
        {
            var sixHoursAgo = DateTime.Now.AddHours(-6);
            var now = DateTime.Now;

            var recentlyClosedPolls = await pollRepository.GetClosingSoonAsync(sixHoursAgo, now);

            var closedPolls = recentlyClosedPolls
                .Where(p => p.EndDate <= now && p.State == "Active")
                .ToList();

            _logger.LogInformation(
                "Encontradas {Count} votaciones cerradas recientemente",
                closedPolls.Count);

            foreach (var poll in closedPolls)
            {
                try
                {
                    await sendPollNotification.ExecuteForClosedAsync(poll.Id);

                    poll.State = "Closed";
                    await pollRepository.UpdateAsync(poll);

                    _logger.LogInformation(
                        "Notificación de cierre enviada para votación {PollId}: {Title}",
                        poll.Id, poll.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al notificar cierre de votación {PollId}",
                        poll.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar votaciones cerradas");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PollNotificationBackgroundService detenido");
        await base.StopAsync(cancellationToken);
    }
}
