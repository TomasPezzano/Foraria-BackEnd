using Foraria.Application.Services;
using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Foraria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpenseController : ControllerBase
{
    private readonly ICreateExpense _createExpense;
    private readonly IGetAllExpenses _getAllExpenses;
    private readonly IPermissionService _permissionService;

    public ExpenseController(
        ICreateExpense createExpense,
        IGetAllExpenses getAllExpenses,
        IPermissionService permissionService)
    {
        _createExpense = createExpense;
        _getAllExpenses = getAllExpenses;
        _permissionService = permissionService;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea una nueva expensa mensual.",
        Description = "Genera una expensa para un consorcio en el mes especificado. Si ya existe una expensa para ese mes, se devuelve un error de negocio."
    )]
    [Authorize(Policy = "ConsortiumAndAdmin")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseDto expenseDto) // si no se puede generar mas de 1 expensa el mismo mes, preguntar si ya hay una creada y devolverla 
    {
        await _permissionService.EnsurePermissionAsync(User, "Expenses.Create");

        if (expenseDto == null)
            throw new DomainValidationException("El cuerpo de la solicitud está vacío.");

        if (!ModelState.IsValid)
            throw new DomainValidationException("Los datos de la expensa no son válidos.");

        if (expenseDto.ConsortiumId <= 0)
            throw new DomainValidationException("El ID del consorcio no es válido.");

        if (string.IsNullOrWhiteSpace(expenseDto.month))
            throw new DomainValidationException("El mes de la expensa es obligatorio (formato 'YYYY-MM').");

        var expense = await _createExpense.ExecuteAsync(expenseDto.ConsortiumId, expenseDto.month);

        if (expense == null)
            throw new BusinessException("No se pudo crear la expensa.");

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
            }).ToList() ?? new List<InvoiceResponseDto>(),
            Residences = expense.Residences?.Select(r => new ResidenceResponseDto
            {
                Id = r.Id,
                Tower = r.Tower,
                Floor = r.Floor,
                Number = r.Number,
                Coeficient = r.Coeficient
            }).ToList() ?? new List<ResidenceResponseDto>()
        };

        return Ok(result);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtiene todas las expensas registradas.",
        Description = "Devuelve la lista de todas las expensas disponibles, incluyendo las facturas asociadas si existen."
    )]
    [Authorize(Policy = "All")]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExpenses()
    {
         await _permissionService.EnsurePermissionAsync(User, "Expenses.ViewAll"); //admin y consorcio. asumo que si trae TODAS las expensas de la base de datos debe ser para los admins

        var expenses = await _getAllExpenses.Execute();

        if (expenses == null)
            throw new NotFoundException("No se pudieron obtener las expensas (resultado nulo).");

        if (!expenses.Any())
            throw new NotFoundException("No se encontraron facturas o expensas registradas.");

        var emptyExpenses = expenses.Where(e => e.Invoices == null || !e.Invoices.Any()).ToList();
        if (emptyExpenses.Any())
        {
            return Ok(new
            {
                message = "Se encontraron expensas, pero algunas no tienen facturas asociadas.",
                totalExpenses = expenses.Count(),
                expensesWithoutInvoices = emptyExpenses.Select(e => e.Id).ToList()
            });
        }

        return Ok(expenses);
    }
}
