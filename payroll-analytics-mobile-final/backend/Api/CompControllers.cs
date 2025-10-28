namespace PayrollAnalytics.Api;

public static class CompControllers
{
    public static object GetCompetitiveness()
    {
        var rnd = new Random(31);
        // points: [tenureYears, compaRatio, level(1-5)]
        var points = Enumerable.Range(0, 150).Select(_ => new [] {
            Math.Round(rnd.NextDouble()*20, 1),
            Math.Round(0.7 + rnd.NextDouble()*0.8, 2),
            rnd.Next(1,6)
        }).ToArray();
        return new { points };
    }

    public static object GetPayGap()
    {
        // Example steps to visualize a wage gap bridge (total delta to close gap)
        var steps = new [] {
            new { label = "Raw Gap", delta = -12.0 },
            new { label = "Job Mix", delta = 4.0 },
            new { label = "Tenure", delta = 2.5 },
            new { label = "Performance", delta = 1.0 },
            new { label = "Geo/Market", delta = 1.5 },
            new { label = "Residual Gap", delta = -3.0 }
        };
        return new { steps };
    }
}
