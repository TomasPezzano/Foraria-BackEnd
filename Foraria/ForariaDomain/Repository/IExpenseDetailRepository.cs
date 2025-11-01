using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Repository;

public interface IExpenseDetailRepository
{
    Task<ExpenseDetailByResidence> AddExpenseDetailAsync(ExpenseDetailByResidence newExpenseDetail);
    Task<ICollection<ExpenseDetailByResidence>> GetExpenseDetailByResidence(int id);
}
