using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foraria.Controllers
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
            try
            {
                
                if (expenseDto == null)
                    return BadRequest("El cuerpo de la solicitud no puede estar vacío.");

                if (expenseDto.ConsortiumId <= 0)
                    return BadRequest("Debe especificar un ConsortiumId válido.");

                if (string.IsNullOrWhiteSpace(expenseDto.month))
                    return BadRequest("Debe especificar un mes válido (por ejemplo, '2025-09').");

                
                var expense = await _getExpenseWithDto.ExecuteAsync(expenseDto.ConsortiumId, expenseDto.month);
                if (expense == null)
                    return NotFound($"No se encontró una expensa para el consorcio ID {expenseDto.ConsortiumId} y el mes {expenseDto.month}.");

                
                var expenseDetails = await _createExpenseDetail.ExecuteAsync(expense);
                if (expenseDetails == null || !expenseDetails.Any())
                    return BadRequest("No se pudieron generar los detalles de expensa.");

                
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
                        ConsortiumId = expense.ConsortiumId,
                        Invoices = expense.Invoices?.Select(i => new InvoiceResponseDto
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = $"Operación inválida: {ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al crear los detalles de expensa.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenseDetailByResidence(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Debe proporcionar un ID de residencia válido.");

                var expenseDetailByResidences = await _getExpenseDetailByResidence.ExecuteAsync(id);

                if (expenseDetailByResidences == null)
                    return NotFound($"No se encontró información de expensas para la residencia con ID {id}.");

                if (!expenseDetailByResidences.Any())
                    return NotFound($"No existen detalles de expensa asociados a la residencia con ID {id}.");

                var result = expenseDetailByResidences.Select(detail => new ExpenseDetailDto
                {
                    Id = detail.Id,
                    Total = detail.TotalAmount,
                    State = detail.State,
                    ResidenceId = detail.ResidenceId,
                    ExpenseId = detail.ExpenseId,
                    Expense = detail.Expense == null ? null : new ExpenseResponseDto
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = $"Operación inválida: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al obtener los detalles de expensa.",
                    error = ex.Message
                });
            }
        }
    }
}
