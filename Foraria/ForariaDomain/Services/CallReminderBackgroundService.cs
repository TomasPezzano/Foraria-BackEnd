using ForariaDomain.Application.UseCase;
using ForariaDomain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Services;

public class CallReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CallReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public CallReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CallReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CallReminderBackgroundService iniciado");

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckUpcomingCallsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CallReminderBackgroundService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("CallReminderBackgroundService detenido");
    }

    private async Task CheckUpcomingCallsAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var callRepository = scope.ServiceProvider.GetRequiredService<ICallRepository>();
        var sendCallNotification = scope.ServiceProvider.GetRequiredService<ISendCallNotification>();

        try
        {
            var activeCalls = callRepository.GetActiveCalls();

            var tomorrow = DateTime.Now.AddDays(1).Date;
            var dayAfterTomorrow = tomorrow.AddDays(1);

            var upcomingCalls = activeCalls
                .Where(c => c.StartedAt >= tomorrow && c.StartedAt < dayAfterTomorrow)
                .ToList();

            _logger.LogInformation(
                "Encontradas {Count} reuniones para mañana",
                upcomingCalls.Count);

            foreach (var call in upcomingCalls)
            {
                try
                {
                    await sendCallNotification.ExecuteForCallReminderAsync(call.Id);

                    _logger.LogInformation(
                        "Recordatorio enviado para reunión {CallId} programada para {StartedAt}",
                        call.Id, call.StartedAt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al enviar recordatorio para reunión {CallId}",
                        call.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar reuniones próximas");
        }
    }
}