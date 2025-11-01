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

    public async Task<Expense> ExecuteAsync(int ConsortiumId, string Date)
    {
        return await _expenseRepository.GetExpenseByConsortiumAndMonthAsync(ConsortiumId, Date);
    }
}
