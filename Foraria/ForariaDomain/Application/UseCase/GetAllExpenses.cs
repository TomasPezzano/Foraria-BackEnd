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
        return await _expensesRepository.GetAllExpenses();
    }
}
