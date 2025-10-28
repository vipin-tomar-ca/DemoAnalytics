namespace PayrollAnalytics.Api;

public static class TurnoverControllers
{
    public static object GetCosts()
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM")).ToArray();
        var rnd = new Random(51);
        var replacementCost = labels.Select(_ => rnd.Next(80_000, 260_000)).ToArray();
        var voluntaryPct = labels.Select(_ => Math.Round(0.8 + rnd.NextDouble()*1.5, 2)).ToArray();
        var involuntaryPct = labels.Select(_ => Math.Round(0.3 + rnd.NextDouble()*1.0, 2)).ToArray();
        return new { labels, replacementCost, voluntaryPct, involuntaryPct };
    }
}
