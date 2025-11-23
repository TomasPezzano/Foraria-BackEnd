
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
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
                using var scope = _scopeFactory.CreateScope();

                var pollRepository = scope.ServiceProvider.GetRequiredService<IPollRepository>();
                var getPollEntity = scope.ServiceProvider.GetRequiredService<GetPollWithResults>(); 
                var notarizePoll = scope.ServiceProvider.GetRequiredService<NotarizePoll>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var polls = await pollRepository.GetAllPolls();

                foreach (var poll in polls)
                {
                    if (poll.State == "Activa" && poll.DeletedAt <= DateTime.Now)
                    {
                        poll.State = "Finalizada";
                        await pollRepository.UpdatePoll(poll);


                        var pollEntity = await getPollEntity.ExecuteAsync(poll.Id);

                        if (pollEntity == null)
                            continue;

                        var pollWithResultsDomain = new PollWithResultsDomain
                        {
                            Id = pollEntity.Id,
                            Title = pollEntity.Title,
                            Description = pollEntity.Description,
                            CreatedAt = pollEntity.CreatedAt,
                            DeletedAt = pollEntity.DeletedAt,
                            State = pollEntity.State,
                            CategoryPollId = pollEntity.CategoryPoll_id,
                            PollOptions = pollEntity.PollOptions.Select(o => new PollOptionDomain
                            {
                                Id = o.Id,
                                Text = o.Text
                            }).ToList(),
                            PollResults = pollEntity.Votes
                                .GroupBy(v => v.PollOption_id)
                                .Select(g => new PollResult
                                {
                                    PollOptionId = g.Key,
                                    VotesCount = g.Count()
                                })
                                .ToList()
                        };

                        var json = notarizePoll.BuildNotarizableJson(pollWithResultsDomain);

                        await notarizePoll.ExecuteAsync(poll.Id, json);
                    }
                }

                await uow.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

    }
}
