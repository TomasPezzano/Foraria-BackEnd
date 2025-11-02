using Foraria.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;
public interface IGetAllExpenses {

    Task<IEnumerable<Expense>> Execute();
}
public class GetAllExpenses : IGetAllExpenses
{
    private readonly IExpenseRepository _expensesRepository;

    public GetAllExpenses(IExpenseRepository expensesRepository)
    {
        _expensesRepository = expensesRepository;
    }

    public async Task<IEnumerable<Expense>> Execute()
    {
        try
        {
            var expenses = await _expensesRepository.GetAllExpenses();

            if (expenses == null)
                throw new InvalidOperationException("El repositorio devolvió un valor nulo al obtener las expensas.");

            if (!expenses.Any())
                throw new KeyNotFoundException("No se encontraron expensas registradas.");

            return expenses;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ocurrió un error inesperado al obtener las expensas.", ex);
        }
    }
}
