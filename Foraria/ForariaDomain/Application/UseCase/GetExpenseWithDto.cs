using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;
public interface IGetExpenseWithDto
{
    Task<Expense> ExecuteAsync(int ConsortiumId, string Date);
}
public class GetExpenseWithDto : IGetExpenseWithDto
{
    private readonly IExpenseRepository _expenseRepository;

    public GetExpenseWithDto(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Expense> ExecuteAsync(int consortiumId, string date)
    {
        if (consortiumId <= 0)
            throw new ArgumentException("El ID del consorcio debe ser mayor que cero.", nameof(consortiumId));

        if (string.IsNullOrWhiteSpace(date))
            throw new ArgumentException("Debe especificar un mes válido.", nameof(date));

        try
        {
            var expense = await _expenseRepository.GetExpenseByConsortiumAndMonthAsync(consortiumId, date);

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
