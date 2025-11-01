using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;

public interface ICreateExpenseDetail
{
    Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(Expense expense);
}
public class CreateExpenseDetail : ICreateExpenseDetail
{
    private readonly IExpenseDetailRepository _expenseDetailRepository;
    private readonly IGetAllResidencesByConsortiumWithOwner _getAllResidencesByConsortiumWithOwner;

    public CreateExpenseDetail(IExpenseDetailRepository expenseDetailRepository, IGetAllResidencesByConsortiumWithOwner getAllResidencesByConsortiumWithOwner)
    {
        _expenseDetailRepository = expenseDetailRepository;
        _getAllResidencesByConsortiumWithOwner = getAllResidencesByConsortiumWithOwner;
    }
    public async Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(Expense expense)
    {
       var result = new List<ExpenseDetailByResidence>();

        var residences = await _getAllResidencesByConsortiumWithOwner.ExecuteAsync(expense.ConsortiumId);

        foreach (var residence in residences)
        {
            double residenceShare = expense.TotalAmount * residence.Coeficient; // REVISAR FORMULA "dividido 100?"
            ExpenseDetailByResidence expenseDetail = new ExpenseDetailByResidence
            {
                ExpenseId = expense.Id,
                ResidenceId = residence.Id,
                TotalAmount = residenceShare,
                State = "Pending"
            };
            result.Add(await _expenseDetailRepository.AddExpenseDetailAsync(expenseDetail));
        }

        
        return result;
    }
}
