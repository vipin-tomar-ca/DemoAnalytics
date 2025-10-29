using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.DTOs;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api")]
// [Authorize] // Commented out to disable authentication
public class AnalyticsController : ControllerBase
{
    private readonly PayrollContext _db;

    public AnalyticsController(PayrollContext db) => _db = db;

    [HttpGet("kpis")]
    public async Task<ActionResult<Kpis>> GetKpis([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var periodEnd = to ?? DateTime.UtcNow;
        var periodStart = from ?? periodEnd.AddMonths(-1);

        var headcount = await _db.Employees.CountAsync(e =>
            e.HireDate <= periodEnd && (e.TerminationDate == null || e.TerminationDate > periodEnd));

        var monthStart = new DateTime(periodEnd.Year, periodEnd.Month, 1);
        var hiresMtd = await _db.EmployeeStarts.CountAsync(s => s.StartDate >= monthStart && s.StartDate <= periodEnd);
        var exitsMtd = await _db.EmployeeExits.CountAsync(s => s.ExitDate >= monthStart && s.ExitDate <= periodEnd);

        var overtimeHours = await _db.Absences
            .Where(a => a.IsOvertime && a.Date >= periodStart && a.Date <= periodEnd)
            .SumAsync(a => a.Hours);
        var totalHours = await _db.Absences
            .Where(a => a.Date >= periodStart && a.Date <= periodEnd)
            .SumAsync(a => a.Hours);
        var overtimePct = totalHours > 0 ? (double)overtimeHours * 100.0 / (double)totalHours : 0.0;

        var avgSalary = await _db.Compensations
            .GroupBy(c => c.EmployeeId)
            .Select(g => g.OrderByDescending(c => c.EffectiveDate).First().BaseSalary)
            .DefaultIfEmpty(0m)
            .AverageAsync(v => v);

        var totalCost = await _db.Compensations
            .Where(c => c.EffectiveDate >= periodStart && c.EffectiveDate <= periodEnd)
            .SumAsync(c => c.TotalCompensation);

        var overtimeCost = await _db.Absences
            .Where(a => a.IsOvertime && a.Date >= periodStart && a.Date <= periodEnd)
            .SumAsync(a => a.Cost);

        var newHires = await _db.EmployeeStarts
            .CountAsync(es => es.StartDate >= periodStart && es.StartDate <= periodEnd);

        var terminations = await _db.EmployeeExits
            .CountAsync(ee => ee.ExitDate >= periodStart && ee.ExitDate <= periodEnd);

        var totalEmployees = await _db.Employees.CountAsync();
        var turnoverRate = totalEmployees > 0 ? (decimal)terminations / totalEmployees * 100 : 0;
        var turnoverCost = terminations * 50000; // Assuming $50,000 cost per turnover

        return new Kpis
        {
            OverallCost = totalCost,
            OvertimeCost = overtimeCost,
            OvertimePct = (double)overtimePct,
            AverageSalary = avgSalary,
            Headcount = headcount,
            NewHires = newHires,
            Terminations = terminations,
            TurnoverCost = turnoverCost,
            TurnoverRate = (double)turnoverRate
        };
    }

    [HttpGet("kpis/all")] // New endpoint for all KPIs without date range
    public async Task<ActionResult<Kpis>> GetAllKpis()
    {
        var headcount = await _db.Employees.CountAsync(e => e.IsActive);

        var newHires = await _db.EmployeeStarts.CountAsync();
        var terminations = await _db.Employees.CountAsync(e => e.TerminationDate != null);

        var overtimeHours = await _db.Absences
            .Where(a => a.IsOvertime)
            .SumAsync(a => a.Hours);
        var totalHours = await _db.Absences
            .SumAsync(a => a.Hours);
        var overtimePct = totalHours > 0 ? (double)overtimeHours * 100.0 / (double)totalHours : 0.0;

        var avgSalary = await _db.Compensations
            .GroupBy(c => c.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, LatestCompensation = g.OrderByDescending(c => c.EffectiveDate).First() })
            .ToListAsync(); // Fetch grouped data into memory

        var averageBaseSalary = avgSalary.Select(x => x.LatestCompensation.BaseSalary).DefaultIfEmpty(0m).Average();

        var totalCost = await _db.Compensations
            .SumAsync(c => c.TotalCompensation);

        var turnoverRate = headcount > 0 ? (double)terminations / headcount * 100 : 0;
        var turnoverCost = terminations * 50000; // Assuming $50,000 cost per turnover

        return new Kpis
        {
            OverallCost = totalCost,
            OvertimeCost = await _db.Absences.Where(a => a.IsOvertime).SumAsync(a => a.Cost),
            OvertimePct = overtimePct,
            AverageSalary = averageBaseSalary, // Use the in-memory calculated average
            Headcount = headcount,
            NewHires = newHires,
            Terminations = terminations,
            TurnoverCost = turnoverCost,
            TurnoverRate = turnoverRate
        };
    }

    [HttpGet("headcount/trend")]
    public async Task<ActionResult<IEnumerable<HeadcountTrendDto>>> GetHeadcountTrend([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? org = null)
    {
        var periodEnd = to ?? DateTime.UtcNow;
        var periodStart = from ?? periodEnd.AddMonths(-11);

        var months = Enumerable.Range(0, 1 + (int)(periodEnd - periodStart).TotalDays / 30)
            .Select(i => periodStart.AddMonths(i))
            .Where(m => m <= periodEnd)
            .ToList();

        var trendData = new List<HeadcountTrendDto>();

        foreach (var month in months)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var headcount = await _db.Employees.CountAsync(e => e.HireDate <= endOfMonth && (e.TerminationDate == null || e.TerminationDate > startOfMonth));
            var newHires = await _db.EmployeeStarts.CountAsync(es => es.StartDate >= startOfMonth && es.StartDate <= endOfMonth);
            var terminations = await _db.EmployeeExits.CountAsync(ee => ee.ExitDate >= startOfMonth && ee.ExitDate <= endOfMonth);

            trendData.Add(new HeadcountTrendDto
            {
                Year = month.Year,
                Month = month.Month,
                Headcount = headcount,
                NewHires = newHires,
                Terminations = terminations
            });
        }

        return Ok(trendData);
    }

    [HttpGet("geo/headcount")]
    public async Task<ActionResult<IEnumerable<GeoRow>>> GetGeoHeadcount()
    {
        var geo = await _db.Employees
            .Include(e => e.Location) // Include Location to access Latitude and Longitude
            .GroupBy(e => e.Province)
            .Select(g => new GeoRow
            {
                Name = g.Key ?? "Unknown",
                Value = g.Count(),
                Latitude = g.First().Location != null ? g.First().Location.Latitude : 0.0, // Use actual Latitude, default to 0.0
                Longitude = g.First().Location != null ? g.First().Location.Longitude : 0.0 // Use actual Longitude, default to 0.0
            })
            .ToListAsync();

        return Ok(geo);
    }

    [HttpGet("time/heatmap")]
    public async Task<ActionResult<TimeHeatmapDto>> GetTimeHeatmap()
    {
        string[] days = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"];
        string[] hours = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToArray();
        var values = new List<int[]>();

        var overtimeRecords = await _db.Absences.Where(a => a.IsOvertime).ToListAsync();
        foreach (var record in overtimeRecords)
        {
            var dayIndex = ((int)record.Date.DayOfWeek + 6) % 7;
            var hourIndex = record.Date.Hour;
            values.Add(new[] { hourIndex, dayIndex, (int)Math.Round(record.Hours) });
        }

        return Ok(new TimeHeatmapDto { Department = "Overall", Data = values.GroupBy(v => v[1]).Select(g => g.OrderBy(v => v[0]).Select(v => v[2]).ToList()).ToList() });
    }

    [HttpGet("compensation/summary")]
    public async Task<ActionResult<CompensationDto>> GetCompensationSummary()
    {
        var categories = await _db.Employees
            .Select(e => e.JobFamily)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var boxData = new List<int[]>();
        foreach (var category in categories)
        {
            var latestSalaries = await _db.Employees
                .Where(e => e.JobFamily == category)
                .Join(_db.Compensations, e => e.Id, c => c.EmployeeId, (_, c) => c)
                .GroupBy(c => c.EmployeeId)
                .Select(g => g.OrderByDescending(c => c.EffectiveDate).First().BaseSalary)
                .Select(s => (int)s)
                .OrderBy(s => s)
                .ToListAsync();

            if (latestSalaries.Count == 0)
            {
                boxData.Add(new[] { 0, 0, 0, 0, 0 });
                continue;
            }

            int Quartile(double percentile) =>
                latestSalaries[(int)Math.Clamp(Math.Round(percentile * (latestSalaries.Count - 1)), 0, latestSalaries.Count - 1)];

            boxData.Add(new[]
            {
                latestSalaries.First(),
                Quartile(0.25),
                Quartile(0.5),
                Quartile(0.75),
                latestSalaries.Last()
            });
        }

        return Ok(new CompensationDto
        {
            EmployeeId = 0, // Placeholder
            EmployeeName = "Summary", // Placeholder
            BaseSalary = 0,
            Bonus = 0,
            Commission = 0,
            Benefits = 0,
            TotalCompensation = 0,
            EffectiveDate = DateTime.UtcNow,
            PayGrade = "Overall" // Placeholder
        });
    }
}
