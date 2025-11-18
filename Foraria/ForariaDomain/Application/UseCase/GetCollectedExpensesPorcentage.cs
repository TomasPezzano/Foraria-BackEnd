using Foraria.Domain.Repository;
using System.Globalization;


namespace foraria.application.usecase
{
    public class GetCollectedExpensesPercentage
    {
        private readonly IExpenseRepository _repository;

        public GetCollectedExpensesPercentage(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExecuteAsync(DateTime? date = null)
        {
            var now = date ?? DateTime.Now;
            var monthstart = new DateTime(now.Year, now.Month, 1);
            var monthend = monthstart.AddMonths(1);

            var stats = await _repository.GetMonthlyCollectionStatsAsync(monthstart, monthend);

            double percentage = stats.totalCount == 0
                ? 0
                : Math.Round((double)stats.paidCount / stats.totalCount * 100, 2);

            var culture = new CultureInfo("es-ar");
            var monthname = culture.DateTimeFormat.GetMonthName(now.Month);
            
            return new
            {
                month = $"{monthname} {now.Year}",
                collectedpercentage = percentage,
                paidcount = stats.paidCount,
                totalcount = stats.totalCount,
                totalpaidamount = Math.Round(stats.totalPaidAmount, 2),
                totalunpaidamount = Math.Round(stats.totalUnpaidAmount, 2)
            };
        }
    }
}
