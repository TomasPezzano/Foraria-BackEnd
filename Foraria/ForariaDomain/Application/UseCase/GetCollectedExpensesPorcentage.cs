using Foraria.Domain.Repository;
using System.Globalization;

namespace Foraria.Application.UseCase
{
    public class GetCollectedExpensesPercentage
    {
        private readonly IExpenseRepository _repository;

        public GetCollectedExpensesPercentage(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(int consortiumId, DateTime? date = null)
        {
            var now = date ?? DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var stats = await _repository.GetMonthlyCollectionStatsAsync(consortiumId, monthStart, monthEnd);

            double percentage = stats.totalCount == 0
                ? 0
                : Math.Round((double)stats.paidCount / stats.totalCount * 100, 2);

            var culture = new CultureInfo("es-AR");
            var monthName = culture.DateTimeFormat.GetMonthName(now.Month);

            return new
            {
                consortiumId,
                month = $"{monthName} {now.Year}",
                collectedPercentage = percentage,
                paidCount = stats.paidCount,
                totalCount = stats.totalCount,
                totalPaidAmount = Math.Round(stats.totalPaidAmount, 2),
                totalUnpaidAmount = Math.Round(stats.totalUnpaidAmount, 2)
            };
        }
    }
}
