using Foraria.Domain.Repository;
using System.Globalization;

namespace Foraria.Application.UseCase
{
    public class GetPendingExpenses
    {
        private readonly IExpenseRepository _repository;

        public GetPendingExpenses(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<object>> ExecuteAsync(int consortiumId)
        {
            var now = DateTime.UtcNow;

            var pending = await _repository.GetPendingExpenses(consortiumId);

            // traduce el día al español
            var culture = new CultureInfo("es-AR");

            return pending
                .Select(e => new
                {
                    description = e.Description,
                    totalAmount = e.TotalAmount,
                    expirationDate = e.ExpirationDate.ToString("yyyy-MM-dd"),
                    dayOfWeek = culture.DateTimeFormat.GetDayName(e.ExpirationDate.DayOfWeek)
                        .First().ToString().ToUpper() +
                        culture.DateTimeFormat.GetDayName(e.ExpirationDate.DayOfWeek).Substring(1)
                })
                .OrderBy(e => e.expirationDate)
                .ToList();
        }
    }
}
