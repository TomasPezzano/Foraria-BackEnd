using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ForariaDomain.Services;

public class OldReserveBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OldReserveBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var updater = scope.ServiceProvider.GetRequiredService<IUpdateOldReserves>();
                await updater.Execute();
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); 
        }
    }
}

