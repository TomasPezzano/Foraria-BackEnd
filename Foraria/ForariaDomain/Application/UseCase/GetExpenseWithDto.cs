using Foraria.Domain.Repository;
using ForariaDomain.Services;

namespace ForariaDomain.Application.UseCase;
public interface IGetExpenseWithDto
{
    Task<Expense> ExecuteAsync( string Date);
}
public class GetExpenseWithDto : IGetExpenseWithDto
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ITenantContext _tenantContext;

    public GetExpenseWithDto(IExpenseRepository expenseRepository, ITenantContext tenantContext)
    {
        _expenseRepository = expenseRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Expense> ExecuteAsync( string date)
    {

        var consortiumId = _tenantContext.GetCurrentConsortiumId();
        if (consortiumId <= 0)
            throw new ArgumentException("El ID del consorcio debe ser mayor que cero.", nameof(consortiumId));


        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Debe especificar un mes válido.", nameof(date));

        try
        {
            var expense = await _expenseRepository.GetExpenseByConsortiumAndMonthAsync(date);

            if (expense == null)
                throw new KeyNotFoundException($"No se encontró una expensa para el consorcio con ID {consortiumId} en el mes '{date}'.");

            return expense;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al obtener la expensa desde el repositorio.", ex);
        }
    }
}
