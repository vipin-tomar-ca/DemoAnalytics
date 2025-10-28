using System.Globalization;

namespace PayrollAnalytics.Api;

public static class DataSeeder
{
    public static Kpis GetKpis(DateTime? from=null, DateTime? to=null, string? province=null, string? org=null)
        => new(
            Headcount: 1827,
            HiresMtd: 24,
            ExitsMtd: 11,
            OvertimePct: 6.8,
            AvgSalary: 84750
        );

    public static HeadcountTrendDto GetHeadcountTrend(DateTime? from=null, DateTime? to=null, string? org=null)
    {
        var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i).ToString("MMM yy", CultureInfo.InvariantCulture)).ToList();
        var rnd = new Random(7);
        var headcount = new List<int>();
        var hires = new List<int>();
        var exits = new List<int>();
        int current = 1750;
        foreach (var _ in labels)
        {
            int h = rnd.Next(10, 60);
            int e = rnd.Next(5, 40);
            current += h - e;
            headcount.Add(current);
            hires.Add(h);
            exits.Add(e);
        }
        return new(labels, headcount, hires, exits);
    }

    public static IEnumerable<GeoRow> GetGeo(string? province=null, string? org=null)
    {
        var rnd = new Random(3);
        string[] provs = ["Ontario","Quebec","British Columbia","Alberta","Manitoba","Saskatchewan","Nova Scotia","New Brunswick","Newfoundland and Labrador","Prince Edward Island","Yukon","Northwest Territories","Nunavut" ];
        return provs.Select(p => new GeoRow(p, rnd.Next(20, 600)));
    }

    public static TimeHeatmapDto GetTimeHeatmap(DateTime? from=null, DateTime? to=null, string? org=null)
    {
        string[] days = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"]; 
        string[] hours = Enumerable.Range(0,24).Select(h=>$"{h:00}:00").ToArray();
        var rnd = new Random(9);
        var values = new List<int[]>();
        for (int d=0; d<days.Length; d++)
            for (int h=0; h<hours.Length; h++)
                values.Add(new[]{ h, d, rnd.Next(0, 100) });
        return new(days, hours, values);
    }

    public static CompensationDto GetComp(string? jobFamily=null, string? org=null)
    {
        string[] cats = ["Engineering","Sales","HR","Finance","Operations","Customer Success"]; 
        var rnd = new Random(11);
        var box = new List<int[]>();
        foreach (var _ in cats)
        {
            int min = rnd.Next(45000, 60000);
            int q1 = min + rnd.Next(5000, 15000);
            int median = q1 + rnd.Next(5000, 15000);
            int q3 = median + rnd.Next(5000, 15000);
            int max = q3 + rnd.Next(5000, 20000);
            box.Add(new[]{min,q1,median,q3,max});
        }
        return new(cats, box);
    }
}
