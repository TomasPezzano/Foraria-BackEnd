using Foraria.Contracts.DTOs;
using Foraria.Interface.DTOs;
using ForariaDomain;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Interface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseDetailController : ControllerBase
    {
        private readonly ICreateExpenseDetail _createExpenseDetail;
        private readonly IGetExpenseWithDto _getExpenseWithDto;
        private readonly IGetExpenseDetailByResidence _getExpenseDetailByResidence;

        public ExpenseDetailController(ICreateExpenseDetail createExpenseDetail, IGetExpenseWithDto getExpenseWithDto, IGetExpenseDetailByResidence getExpenseDetailByResidence)
        {
            _createExpenseDetail = createExpenseDetail;
            _getExpenseWithDto = getExpenseWithDto;
            _getExpenseDetailByResidence = getExpenseDetailByResidence;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpenseDetail([FromBody] ExpenseDto expenseDto)
        {

            var expense = await _getExpenseWithDto.ExecuteAsync(expenseDto.ConsortiumId, expenseDto.month);
            Console.WriteLine(expense);
            var expenseDetails = await _createExpenseDetail.ExecuteAsync(expense);

            var result = expenseDetails.Select(detail => new ExpenseDetailDto
            {
                Id = detail.Id,
                Total = detail.TotalAmount,
                State = detail.State,
                ResidenceId = detail.ResidenceId,
                ExpenseId = detail.ExpenseId,
                Expense = new ExpenseResponseDto
                {
                    Id = expense.Id,
                    Description = expense.Description,
                    TotalAmount = expense.TotalAmount,
                    CreatedAt = expense.CreatedAt,
                    ExpirationDate = expense.ExpirationDate,
                    ConsortiumId = expense.ConsortiumId
                }
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenseDetailByResidence(int id) {

            var expenseDetailByResidences = await _getExpenseDetailByResidence.ExecuteAsync(id);

            var result = expenseDetailByResidences.Select(detail => new ExpenseDetailDto
            {
                Id = detail.Id,
                Total = detail.TotalAmount,
                State = detail.State,
                ResidenceId = detail.ResidenceId,
                ExpenseId = detail.ExpenseId,
                Expense = new ExpenseResponseDto
                {
                    Id = detail.Expense.Id,
                    Description = detail.Expense.Description,
                    TotalAmount = detail.Expense.TotalAmount,
                    CreatedAt = detail.Expense.CreatedAt,
                    ExpirationDate = detail.Expense.ExpirationDate,
                    ConsortiumId = detail.Expense.ConsortiumId,
                    Invoices = detail.Expense.Invoices?.Select(i => new InvoiceResponseDto
                    {
                        Id = i.Id,
                        Category = i.Category,
                        Description = i.Description,
                        Amount = i.Amount
                    }).ToList() ?? new List<InvoiceResponseDto>()
                }
            }).ToList();

            return Ok(result);

        }
    }
}
