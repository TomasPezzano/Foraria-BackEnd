using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface IGetExpenseDetailByResidence 
{
    Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(int id);
}
public class GetExpenseDetailByResidence : IGetExpenseDetailByResidence
{
    private readonly IExpenseDetailRepository _expenseDetailRepository;

    public GetExpenseDetailByResidence(IExpenseDetailRepository expenseDetailRepository) {
        _expenseDetailRepository = expenseDetailRepository;
    }

    public async Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(int id)
    {
        try
        {
            if (id <= 0)
                throw new ArgumentException("El ID de la residencia no es válido.", nameof(id));

            var expenseDetails = await _expenseDetailRepository.GetExpenseDetailByResidence(id);

            if (expenseDetails == null)
                throw new InvalidOperationException("El repositorio devolvió un valor nulo al obtener los detalles de expensa.");

            if (!expenseDetails.Any())
                throw new KeyNotFoundException($"No se encontraron detalles de expensa para la residencia con ID {id}.");

            return expenseDetails;
        }
        catch (ArgumentException)
        {
            throw; 
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
            throw new InvalidOperationException("Ocurrió un error inesperado al obtener los detalles de expensa.", ex);
        }
    }

}
