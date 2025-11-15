using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Application.UseCase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForariaDomain.Services;

public class ExpenseReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpenseReminderBackgroundService> _logger;

    public ExpenseReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExpenseReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpenseReminderBackgroundService iniciado");

        // Esperar 10 segundos antes de la primera ejecución (permite que la app termine de inicializarse)
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar recordatorios de expensas");
            }

            // Ejecutar cada 1 hora
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("ExpenseReminderBackgroundService detenido");
    }

    private async Task CheckAndSendRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var expenseRepository = scope.ServiceProvider.GetRequiredService<IExpenseRepository>();
        var sendReminder = scope.ServiceProvider.GetRequiredService<ISendExpenseReminderNotification>();

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var today = DateTime.UtcNow.Date;

        // Obtener expensas que vencen hoy o mañana
        var expiringExpenses = await expenseRepository.GetExpensesExpiringBetweenAsync(today, tomorrow.AddDays(1));

        if (!expiringExpenses.Any())
        {
            _logger.LogInformation("No hay expensas por vencer en las próximas 24 horas");
            return;
        }

        _logger.LogInformation(
            "Procesando {Count} expensas que vencen pronto",
            expiringExpenses.Count());

        foreach (var expense in expiringExpenses)
        {
            try
            {
                await sendReminder.ExecuteAsync(expense.Id);

                _logger.LogInformation(
                    "Recordatorio enviado para expensa {ExpenseId} - {Description}",
                    expense.Id,
                    expense.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al enviar recordatorio para expensa {ExpenseId}",
                    expense.Id);
            }
        }
    }
}