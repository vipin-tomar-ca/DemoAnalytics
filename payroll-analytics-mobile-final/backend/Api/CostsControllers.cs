namespace PayrollAnalytics.Api;

public static class CostsControllers
{
    public static object GetTcow()
    {
        var rnd = new Random(21);
        var items = new[] {
            ("Base Pay", 14_200_000),
            ("Bonuses", 1_250_000),
            ("Benefits", 3_100_000),
            ("Payroll Taxes", 2_050_000),
            ("Training", 420_000),
            ("Travel", 310_000)
        };
        return new {
            total = items.Sum(i => i.Item2),
            breakdown = items.Select(i => new { name = i.Item1, value = i.Item2 })
        };
    }

    public static object GetBudgetVariance()
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM")).ToArray();
        var rnd = new Random(22);
        var budget = labels.Select(_ => rnd.Next(1400000, 1600000)).ToArray();
        var actual = budget.Select(b => (int)(b * (0.95 + rnd.NextDouble()*0.15))).ToArray();
        var variancePct = actual.Zip(budget, (a,b) => Math.Round((a - b) * 100.0 / b, 1)).ToArray();
        return new { labels, budget, actual, variancePct };
    }

    public static object GetOvertime()
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM")).ToArray();
        var rnd = new Random(23);
        var costs = labels.Select(_ => rnd.Next(45_000, 120_000)).ToArray();
        var hours = labels.Select(_ => rnd.Next(1_000, 3_800)).ToArray();
        return new { labels, costs, hours };
    }

    public static object GetAbsenteeism()
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM")).ToArray();
        var rnd = new Random(24);
        var costs = labels.Select(_ => rnd.Next(30_000, 90_000)).ToArray();
        var ratePct = labels.Select(_ => Math.Round(1.5 + rnd.NextDouble()*3.5, 2)).ToArray();
        return new { labels, costs, ratePct };
    }
}
