using Foraria.Application.UseCase;
using Foraria.Domain.Repository;
using Foraria.Domain.Service;
using ForariaDomain.Repository;

namespace ForariaDomain.Application.UseCase;

public interface ICreateExpenseDetail
{
    Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(Expense expense);
}
public class CreateExpenseDetail : ICreateExpenseDetail
{
    private readonly IExpenseDetailRepository _expenseDetailRepository;
    private readonly IResidenceRepository _residenceRepository;
    private readonly IGetAllResidencesByConsortiumWithOwner _getAllResidencesByConsortiumWithOwner;
    private readonly ISendEmail _emailService;
    private readonly IConfigureNotificationPreferences _configurePreferences;

    public CreateExpenseDetail(IExpenseDetailRepository expenseDetailRepository, IGetAllResidencesByConsortiumWithOwner getAllResidencesByConsortiumWithOwner,
        IResidenceRepository residenceRepository, ISendEmail sendEmail, IConfigureNotificationPreferences configurePreferences)
    {
        _expenseDetailRepository = expenseDetailRepository;
        _getAllResidencesByConsortiumWithOwner = getAllResidencesByConsortiumWithOwner;
        _residenceRepository = residenceRepository;
        _emailService = sendEmail;
        _configurePreferences = configurePreferences;
    }
    public async Task<ICollection<ExpenseDetailByResidence>> ExecuteAsync(Expense expense)
    {
        try
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense), "La expensa no puede ser nula.");

            if (expense.Id <= 0)
                throw new ArgumentException("El ID de la expensa no es válido.", nameof(expense.Id));

            if (expense.ConsortiumId <= 0)
                throw new ArgumentException("El ID del consorcio no es válido.", nameof(expense.ConsortiumId));

            if (expense.TotalAmount <= 0)
                throw new InvalidOperationException("El monto total de la expensa debe ser mayor que cero.");

            var residences = await _getAllResidencesByConsortiumWithOwner.ExecuteAsync();

            if (residences == null)
                throw new InvalidOperationException("No se pudieron obtener las residencias del consorcio.");

            if (!residences.Any())
                throw new KeyNotFoundException($"No se encontraron residencias para el consorcio con ID {expense.ConsortiumId}.");

            var result = new List<ExpenseDetailByResidence>();

            foreach (var residence in residences)
            {
                decimal amountInvoices = 0;
                if (residence.Coeficient <= 0)
                    throw new InvalidOperationException($"La residencia con ID {residence.Id} tiene un coeficiente inválido ({residence.Coeficient}).");

                var invoices = await _residenceRepository.GetInvoicesByResidenceIdAsync(residence.Id, expense.CreatedAt);

                foreach (Invoice invoice in invoices) { 
                    amountInvoices += invoice.Amount;
                }
                double amountInvoicesDouble = (double)amountInvoices;

                var total = expense.TotalAmount + amountInvoicesDouble;

                double residenceShare = total * residence.Coeficient;

                var expenseDetail = new ExpenseDetailByResidence
                {
                    ResidenceId = residence.Id,
                    TotalAmount = residenceShare,
                    State = "Pending",
                    Expenses = new List<Expense>()
                };

                expenseDetail.Expenses.Add(expense);

                var createdDetail = await _expenseDetailRepository.AddExpenseDetailAsync(expenseDetail);

                if (createdDetail == null)
                    throw new InvalidOperationException($"No se pudo crear el detalle de expensa para la residencia {residence.Id}.");

                var owner = residence.Users?.FirstOrDefault(u => u.Role.Description.Equals("Propietario"));

                if (owner != null && !string.IsNullOrWhiteSpace(owner.Mail))
                {
                    var preferences = await _configurePreferences.GetPreferencesAsync(owner.Id);

                    if (preferences != null &&
                        preferences.EmailEnabled &&
                        preferences.ExpenseNotificationsEnabled)
                    {
                        await _emailService.SendExpenseEmail(
                            owner.Mail,
                            $"{owner.Name} {owner.LastName}",
                            residenceShare,
                            expense.CreatedAt.ToString("yyyy-MM")
                        );
                    }
                }

                result.Add(createdDetail);
            }

            return result;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ocurrió un error inesperado al crear los detalles de expensa.", ex);
        }
    }
}
