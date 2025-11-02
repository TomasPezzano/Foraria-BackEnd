//using Foraria.Domain.Repository;
//using System.Globalization;

//namespace Foraria.Application.UseCase
//{
//    public class GetUserMonthlyExpenseHistory
//    {
//        private readonly IExpenseRepository _repository;

//        public GetUserMonthlyExpenseHistory(IExpenseRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<object> ExecuteAsync(int userId, int? year = null)
//        {
//            var now = DateTime.UtcNow;
//            var targetYear = year ?? now.Year;
//            var startOfYear = new DateTime(targetYear, 1, 1);
//            var endOfYear = startOfYear.AddYears(1);

//            var expenses = await _repository.GetUserExpenses(userId);

//            var paidExpenses = expenses
//                .Where(e => e.State == "Paid" && e.CreatedAt >= startOfYear && e.CreatedAt < endOfYear)
//                .ToList();

//            var culture = new CultureInfo("es-AR");

//            var monthlyGroups = paidExpenses
//                .GroupBy(e => e.CreatedAt.Month)
//                .Select(g => new
//                {
//                    Month = g.Key,
//                    TotalPaid = g.Sum(e => e.TotalAmount),
//                    Categories = g.GroupBy(e => e.Category)
//                        .Select(cg => new
//                        {
//                            Category = cg.Key,
//                            Total = cg.Sum(e => e.TotalAmount)
//                        })
//                        .OrderByDescending(c => c.Total)
//                        .ToList()
//                })
//                .ToDictionary(x => x.Month, x => x);

//            var monthlyData = Enumerable.Range(1, 12)
//                .Select(m => new
//                {
//                    month = culture.DateTimeFormat.GetMonthName(m),
//                    totalPaid = Math.Round(monthlyGroups.ContainsKey(m) ? monthlyGroups[m].TotalPaid : 0, 2),
//                    categories = monthlyGroups.ContainsKey(m)
//                    ? monthlyGroups[m].Categories
//                        .Select(c => new CategoryTotalDto { Category = c.Category, Total = Math.Round(c.Total, 2) })
//                        .ToList()
//                    : new List<CategoryTotalDto>()
//                });

//            var totals = monthlyData.Select(x => x.totalPaid).ToList();
//            var activeMonths = totals.Where(x => x > 0).ToList();

//            double average = activeMonths.Count > 0 ? activeMonths.Average() : 0;
//            double highest = activeMonths.Count > 0 ? activeMonths.Max() : 0;
//            double lowest = activeMonths.Count > 0 ? activeMonths.Min() : 0;
//            double variation = (highest > 0 && lowest > 0)
//                ? ((highest - lowest) / lowest) * 100
//                : 0;

//            string highestMonth = highest > 0
//                ? monthlyData.First(x => x.totalPaid == highest).month
//                : "-";

//            string lowestMonth = lowest > 0
//                ? monthlyData.First(x => x.totalPaid == lowest).month
//                : "-";

//            return new
//            {
//                userId,
//                year = targetYear,
//                averageMonthlyPaid = Math.Round(average, 2),
//                variationPercent = Math.Round(variation, 2),
//                highestMonth,
//                lowestMonth,
//                monthlyHistory = monthlyData
//            };
//        }
//    }
//}
