namespace PayrollAnalytics.Api;

public record Kpis(int Headcount, int HiresMtd, int ExitsMtd, double OvertimePct, double AvgSalary);

public record HeadcountTrendDto(IReadOnlyList<string> Labels, IReadOnlyList<int> Headcount, IReadOnlyList<int> Hires, IReadOnlyList<int> Exits);

public record GeoRow(string name, int value);

public record TimeHeatmapDto(IReadOnlyList<string> Days, IReadOnlyList<string> Hours, IReadOnlyList<int[]> Values);

public record CompensationDto(IReadOnlyList<string> Categories, IReadOnlyList<int[]> BoxData);
