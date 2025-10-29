using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class TimeHeatmapDto
    {
        public string Department { get; set; } = "";
        public List<List<int>> Data { get; set; } = new List<List<int>>();
    }
}
