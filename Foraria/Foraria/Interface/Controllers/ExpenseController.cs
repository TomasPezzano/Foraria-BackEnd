using Foraria.Application.UseCase;
using Foraria.Contracts.DTOs;
using ForariaDomain.Application.UseCase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Foraria.Interface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpenseController : ControllerBase
{
    private readonly ICreateExpense _createExpense; 
    private readonly IGetAllExpenses _getAllExpenses;

    public ExpenseController(ICreateExpense createExpense, IGetAllExpenses getAllExpenses)
    {
        _createExpense = createExpense;
        _getAllExpenses = getAllExpenses;
    }


    [HttpPost]
  public async Task<IActionResult> CreateExpense([FromBody] ExpenseDto expenseDto) // si no se puede generar mas de 1 expensa el mismo mes, preguntar si ya hay una creada y devolverla 
  {
        if (expenseDto == null)
            return BadRequest("El cuerpo de la solicitud está vacío.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            
            if (expenseDto.ConsortiumId <= 0)
                return BadRequest("El ID del consorcio no es válido.");

            if (string.IsNullOrWhiteSpace(expenseDto.month))
                return BadRequest("El mes de la expensa es obligatorio (formato 'YYYY-MM').");


            var expense = await _createExpense.ExecuteAsync(expenseDto.ConsortiumId, expenseDto.month);

            if (expense == null)
                return BadRequest("No se pudo crear la expensa.");

            var result = new ExpenseResponseDto
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
                    Description = i.Description,
                    Amount = i.Amount,
                    DateOfIssue = i.DateOfIssue
                }).ToList() ?? new List<InvoiceResponseDto>()
            };

            return Ok(result);
        }
        catch (FormatException)
        {
            return BadRequest("El formato de fecha no es válido. Usa 'YYYY-MM' (por ejemplo, '2025-10').");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }

  }

    [HttpGet]
    public async Task<IActionResult> GetAllInvoices()
    {
        var expenses = await _getAllExpenses.Execute();
        if (expenses == null || !expenses.Any())
            return NotFound("No se encontraron facturas.");
        return Ok(expenses);

    }


}