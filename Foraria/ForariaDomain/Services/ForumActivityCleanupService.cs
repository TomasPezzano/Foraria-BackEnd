using Foraria.Domain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Services;

public class ForumActivityCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ForumActivityCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(7); 

    public ForumActivityCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ForumActivityCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ForumActivityCleanupService iniciado");

        await Task.Delay(TimeSpan.FromHours(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupInactiveThreadsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ForumActivityCleanupService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("ForumActivityCleanupService detenido");
    }

    private async Task CleanupInactiveThreadsAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var threadRepository = scope.ServiceProvider.GetRequiredService<IThreadRepository>();

        try
        {
            var allThreads = await threadRepository.GetAllAsync();

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var inactiveThreads = allThreads
                .Where(t =>
                    (t.UpdatedAt ?? t.CreatedAt) < threeMonthsAgo &&
                    t.State.ToLower() == "open")
                .ToList();

            _logger.LogInformation(
                "Encontrados {Count} threads inactivos por más de 3 meses",
                inactiveThreads.Count);

            foreach (var thread in inactiveThreads)
            {
                thread.State = "Archived";
                await threadRepository.UpdateAsync(thread);

                _logger.LogInformation(
                    "Thread {ThreadId} - '{Theme}' marcado como archivado",
                    thread.Id,
                    thread.Theme);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar threads inactivos");
        }
    }
}