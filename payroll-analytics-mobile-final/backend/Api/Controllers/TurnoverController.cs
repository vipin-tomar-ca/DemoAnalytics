using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/turnover")]
// [Authorize] // Commented out to disable authentication
public class TurnoverController : ControllerBase
{
    private readonly PayrollContext _db;

    public TurnoverController(PayrollContext db) => _db = db;

    [HttpGet("costs")]
    public async Task<IActionResult> GetCosts()
    {
        var months = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
        var labels = months.Select(d => d.ToString("MMM")).ToArray();

        var replacementCost = new List<int>();
        var voluntaryPct = new List<double>();
        var involuntaryPct = new List<double>();

        foreach (var month in months)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
            var exits = await _db.EmployeeExits
                .Where(e => e.ExitDate >= start && e.ExitDate <= end)
                .ToListAsync();

            replacementCost.Add(exits.Count * 25_000);

            var voluntary = exits.Count(e => e.Type == "Voluntary");
            var involuntary = exits.Count - voluntary;
            var employeeCount = await _db.Employees.CountAsync(e =>
                e.HireDate <= end && (e.TerminationDate == null || e.TerminationDate > end));

            double Rate(int numerator) =>
                employeeCount > 0 ? Math.Round(numerator * 100.0 / employeeCount, 2) : 0.0;

            voluntaryPct.Add(Rate(voluntary));
            involuntaryPct.Add(Rate(involuntary));
        }

        return Ok(new { labels, replacementCost, voluntaryPct, involuntaryPct });
    }
}
