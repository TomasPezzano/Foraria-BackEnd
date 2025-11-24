using Foraria.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;
public interface IGetPercentageByCategoryInExpense
{
    Task<Dictionary<string, decimal>> ExecuteAsync();
}
public class GetPercentageByCategoryInExpense : IGetPercentageByCategoryInExpense
{
    private readonly IExpenseRepository _expenseRepository;

    public GetPercentageByCategoryInExpense(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Dictionary<string, decimal>> ExecuteAsync()
    {
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;

        var expenses = await _expenseRepository.GetExpensesByUserAndDateRangeAsync(year, month);

        if (expenses?.Invoices == null || expenses.Invoices.Count == 0)
            return new Dictionary<string, decimal>();

        var invoices = expenses.Invoices;

        var total = invoices.Sum(i => i.Amount);

        if (total == 0)
            return invoices
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => 0m);

        return invoices
            .GroupBy(i => i.Category)
            .ToDictionary(
                g => g.Key,
                g => (g.Sum(x => x.Amount) / total) * 100
            );
    }
}
