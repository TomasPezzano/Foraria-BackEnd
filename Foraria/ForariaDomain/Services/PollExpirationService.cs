
using Foraria.Domain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ForariaDomain.Services
{
    public class PollExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PollExpirationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var pollRepository = scope.ServiceProvider.GetRequiredService<IPollRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var polls = await pollRepository.GetAllPolls();

                    foreach (var poll in polls)
                    {
                        if (poll.State == "Activa" && poll.DeletedAt <= DateTime.UtcNow)
                        {
                            poll.State = "Finalizada";
                            await pollRepository.UpdatePoll(poll);
                        }
                    }

                    await unitOfWork.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); 
            }
        }
    }
}
