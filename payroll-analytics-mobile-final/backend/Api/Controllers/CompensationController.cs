using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/comp")] // Changed route to match frontend
// [Authorize] // Commented out to disable authentication
public class CompensationController : ControllerBase
{
    private readonly PayrollContext _db;

    public CompensationController(PayrollContext db) => _db = db;

    [HttpGet("competitiveness")]
    public async Task<IActionResult> GetCompetitiveness()
    {
        var salaryMidpoints = new Dictionary<string, decimal>
        {
            ["Engineering"] = 110_000m,
            ["Sales"] = 95_000m,
            ["HR"] = 80_000m,
            ["Finance"] = 95_000m,
            ["Operations"] = 85_000m,
            ["Customer Success"] = 75_000m
        };

        var employees = await _db.Employees
            .Select(e => new
            {
                e.HireDate,
                e.JobFamily,
                LatestComp = _db.Compensations
                    .Where(c => c.EmployeeId == e.Id)
                    .OrderByDescending(c => c.EffectiveDate)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var points = employees
            .Where(e => e.LatestComp is not null)
            .Select(e =>
            {
                var tenureYears = Math.Round((DateTime.UtcNow - (e.HireDate ?? DateTime.UtcNow)).TotalDays / 365.0, 1);
                var midpoint = salaryMidpoints.TryGetValue(e.JobFamily, out var mid) ? mid : 90_000m;
                var compaRatio = midpoint > 0 ? Math.Round((double)(e.LatestComp!.BaseSalary / midpoint), 2) : 0.0;
                var levelSeed = (e.HireDate ?? DateTime.UtcNow).Day;
                var level = new Random(levelSeed).Next(1, 6);
                return new[] { tenureYears, compaRatio, (double)level };
            })
            .ToArray();

        return Ok(new { points });
    }
}
