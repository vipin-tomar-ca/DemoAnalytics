using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/costs")]
// [Authorize] // Commented out to disable authentication
public class CostsController : ControllerBase
{
    private readonly PayrollContext _db;

    public CostsController(PayrollContext db) => _db = db;

    [HttpGet("tcow")]
    public async Task<IActionResult> GetTotalCostOfWorkforce()
    {
        var latestComp = await _db.Compensations
            .GroupBy(c => c.EmployeeId)
            .Select(g => g.OrderByDescending(c => c.EffectiveDate).First())
            .ToListAsync();

        var totalBase = latestComp.Sum(c => c.BaseSalary);
        var totalBonus = latestComp.Sum(c => c.Bonus);
        var totalBenefits = latestComp.Sum(c => c.Benefits);
        var totalTaxes = latestComp.Sum(c => c.PayrollTaxes);
        var training = (decimal)(latestComp.Count * 800);
        var travel = (decimal)(latestComp.Count * 500);

        return Ok(new
        {
            total = totalBase + totalBonus + totalBenefits + totalTaxes + training + travel,
            breakdown = new object[]
            {
                new { name = "Base Pay", value = totalBase },
                new { name = "Bonuses", value = totalBonus },
                new { name = "Benefits", value = totalBenefits },
                new { name = "Payroll Taxes", value = totalTaxes },
                new { name = "Training", value = training },
                new { name = "Travel", value = travel }
            }
        });
    }

    [HttpGet("budget-variance")]
    public IActionResult GetBudgetVariance()
    {
        var months = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
        var labels = months.Select(d => d.ToString("MMM")).ToArray();

        var rnd = new Random(22);
        var budget = labels.Select(_ => rnd.Next(1_400_000, 1_600_000)).ToArray();
        var actual = budget.Select(b => (int)(b * (0.9 + rnd.NextDouble() * 0.2))).ToArray();
        var variancePct = actual
            .Zip(budget, (actualValue, budgetValue) => Math.Round((actualValue - budgetValue) * 100.0 / budgetValue, 1))
            .ToArray();

        return Ok(new { labels, budget, actual, variancePct });
    }

    [HttpGet("overtime")]
    public async Task<IActionResult> GetOvertime()
    {
        var months = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
        var labels = months.Select(d => d.ToString("MMM")).ToArray();

        var costs = new List<int>();
        var hours = new List<int>();

        foreach (var month in months)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
            var overtimeRecords = await _db.Absences
                .Where(a => a.IsOvertime && a.Date >= start && a.Date <= end)
                .ToListAsync();
            costs.Add((int)overtimeRecords.Sum(a => a.Cost));
            hours.Add((int)overtimeRecords.Sum(a => a.Hours));
        }

        return Ok(new { labels, costs, hours });
    }

    [HttpGet("absenteeism")]
    public async Task<IActionResult> GetAbsenteeism()
    {
        var months = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
        var labels = months.Select(d => d.ToString("MMM")).ToArray();

        var costs = new List<int>();
        var ratePct = new List<double>();

        foreach (var month in months)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
            var absences = await _db.Absences
                .Where(a => !a.IsOvertime && a.Date >= start && a.Date <= end)
                .ToListAsync();

            costs.Add((int)absences.Sum(a => a.Cost));
            var employeeCount = await _db.Employees.CountAsync(e =>
                e.HireDate <= end && (e.TerminationDate == null || e.TerminationDate > end));
            var totalHours = employeeCount * 160.0;
            var rate = totalHours > 0 ? Math.Round((double)absences.Sum(a => a.Hours) * 100.0 / totalHours, 2) : 0.0;
            ratePct.Add(rate);
        }

        return Ok(new { labels, costs, ratePct });
    }
}
