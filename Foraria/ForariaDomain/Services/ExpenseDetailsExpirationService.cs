using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Models;
using ForariaDomain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ForariaDomain.Services
{
    public class ExpenseDetailsExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpenseDetailsExpirationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("ejecutando proceso de expensedetail");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var expenseDetailRepository = scope.ServiceProvider.GetRequiredService<IExpenseDetailRepository>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expenseDetails = await expenseDetailRepository.GetAllExpenseDetailsByStateAsync("Pending");

                foreach (var expenseDetail in expenseDetails)
                {
                    if (expenseDetail.State == "Pending" && expenseDetail.ExpirationDate <= DateTime.UtcNow)
                    {
                        expenseDetail.State = "Expired";
                        await expenseDetailRepository.UpdateExpenseDetailAsync(expenseDetail);
                    }
                }

                await uow.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}

