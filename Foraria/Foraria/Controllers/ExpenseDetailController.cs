using Foraria.DTOs;
using ForariaDomain.Application.UseCase;
using ForariaDomain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(
            Summary = "Genera los detalles de expensa para un consorcio y mes específico.",
            Description = "Crea automáticamente los registros de detalle de expensa según las facturas asociadas al consorcio y al mes indicado."
        )]
        [ProducesResponseType(typeof(IEnumerable<ExpenseDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateExpenseDetail([FromBody] ExpenseDto expenseDto)
        {
            if (expenseDto == null)
                throw new DomainValidationException("El cuerpo de la solicitud no puede estar vacío.");

            if (expenseDto.ConsortiumId <= 0)
                throw new DomainValidationException("Debe especificar un ConsortiumId válido.");

            if (string.IsNullOrWhiteSpace(expenseDto.month))
                throw new DomainValidationException("Debe especificar un mes válido (por ejemplo, '2025-09').");

            var expense = await _getExpenseWithDto.ExecuteAsync(expenseDto.ConsortiumId, expenseDto.month);
            if (expense == null)
                throw new NotFoundException($"No se encontró una expensa para el consorcio ID {expenseDto.ConsortiumId} y el mes {expenseDto.month}.");

            var expenseDetails = await _createExpenseDetail.ExecuteAsync(expense);
            if (expenseDetails == null || !expenseDetails.Any())
                throw new BusinessException("No se pudieron generar los detalles de expensa.");

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

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene los detalles de expensa asociados a una residencia.",
            Description = "Devuelve la lista de detalles de expensa vinculados a la residencia indicada por su ID."
        )]
        [ProducesResponseType(typeof(IEnumerable<ExpenseDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllExpenseDetailByResidence(int id)
        {
            if (id <= 0)
                throw new DomainValidationException("Debe proporcionar un ID de residencia válido.");

            var expenseDetailByResidences = await _getExpenseDetailByResidence.ExecuteAsync(id);

            if (expenseDetailByResidences == null)
                throw new NotFoundException($"No se encontró información de expensas para la residencia con ID {id}.");

            if (!expenseDetailByResidences.Any())
                throw new NotFoundException($"No existen detalles de expensa asociados a la residencia con ID {id}.");

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
    }
}
