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

    public async Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(int id) {

       return  await _expenseDetailRepository.GetExpenseDetailByResidence(id);

    }

}
