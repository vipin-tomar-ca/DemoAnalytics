using System.Globalization;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.DTOs;

namespace PayrollAnalytics.Api.Data;

public static class DataSeeder
{
    public static Kpis GetKpis(DateTime? from=null, DateTime? to=null, string? province=null, string? org=null)
        => new Kpis
        {
            Headcount = 1827,
            NewHires = 24,
            Terminations = 11,
            OvertimePct = 6.8,
            AverageSalary = 84750
        };

    public static IEnumerable<HeadcountTrendDto> GetHeadcountTrend(DateTime? from=null, DateTime? to=null, string? org=null)
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM yy", CultureInfo.InvariantCulture)).ToList();
        var rnd = new Random(7);
        var currentHeadcount = 1800;
        var trend = new List<HeadcountTrendDto>();

        foreach (var label in labels)
        {
            var hires = rnd.Next(20, 50);
            var terminations = rnd.Next(10, 30);
            currentHeadcount += (hires - terminations);
            trend.Add(new HeadcountTrendDto
            {
                Year = DateTime.ParseExact(label, "MMM yy", CultureInfo.InvariantCulture).Year,
                Month = DateTime.ParseExact(label, "MMM yy", CultureInfo.InvariantCulture).Month,
                Headcount = currentHeadcount,
                NewHires = hires,
                Terminations = terminations
            });
        }
        return trend;
    }

    public static TimeHeatmapDto GetTimeHeatmap(DateTime? from=null, DateTime? to=null, string? org=null)
    {
        string[] departments = ["Sales", "Engineering", "Ops", "Support"];
        var rnd = new Random(9);
        var data = new List<List<int>>();

        foreach (var dept in departments)
        {
            var row = new List<int>();
            for (int i = 0; i < 7 * 24; i++) // 7 days, 24 hours
            {
                row.Add(rnd.Next(0, 100));
            }
            data.Add(row);
        }

        return new TimeHeatmapDto
        {
            Department = org ?? "Overall",
            Data = data
        };
    }

    public static CompensationDto GetComp(string? jobFamily=null, string? org=null)
    {
        return new CompensationDto
        {
            EmployeeId = 1,
            EmployeeName = "John Doe",
            BaseSalary = 80000m,
            Bonus = 10000m,
            Commission = 5000m,
            Benefits = 15000m,
            TotalCompensation = 110000m,
            EffectiveDate = DateTime.UtcNow,
            PayGrade = "Grade 5"
        };
    }

    public static async Task SeedDataAsync(PayrollContext context)
    {
        // This method can be used for additional data seeding if needed
        // For now, the main seeding logic is in Seed.EnsureDbAsync
        await Task.CompletedTask;
    }
}
