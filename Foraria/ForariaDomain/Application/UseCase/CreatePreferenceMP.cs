using Foraria.Contracts.DTOs;
using Foraria.Domain.Repository;
using ForariaDomain.Repository;
using ForariaDomain.Services;

namespace ForariaDomain.Application.UseCase
{
    public class CreatePreferenceMP
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGateway _paymentGateway;

        public CreatePreferenceMP(
            IExpenseRepository expenseRepository,
            IPaymentRepository paymentRepository,
            IPaymentGateway paymentGateway)
        {
            _expenseRepository = expenseRepository;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task<CreatePreferenceResponse> ExecuteAsync(int expenseId, int residenceId)
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);

            if (expense == null)
                throw new Exception("Expense no encontrada.");

            var amount = expense.TotalAmount;

            var (preferenceId, initPoint) =
                await _paymentGateway.CreatePreferenceAsync((decimal)amount, expenseId, residenceId);

            var payment = new Payment
            {
                PreferenceId = preferenceId,
                Date = DateTime.UtcNow,
                ExpenseId = expenseId,
                ResidenceId = residenceId,
                Status = "pending",
                Amount = (decimal)amount
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            return new CreatePreferenceResponse(preferenceId, initPoint);
        }
    }
}
