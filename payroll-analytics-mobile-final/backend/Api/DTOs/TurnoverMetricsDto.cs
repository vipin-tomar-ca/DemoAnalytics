using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class TurnoverMetricsDto
    {
        public int Year { get; set; }
        public int TotalHires { get; set; }
        public int TotalTerminations { get; set; }
        public int TotalExits { get; set; }
        public int VoluntaryExits { get; set; }
        public int InvoluntaryExits { get; set; }
        public decimal TurnoverRate { get; set; }
        public decimal OverallTurnoverRate { get; set; }
        public decimal VoluntaryTurnoverRate { get; set; }
        public decimal InvoluntaryTurnoverRate { get; set; }
        public decimal TurnoverCost { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<DepartmentTurnoverDto> ByDepartment { get; set; } = new List<DepartmentTurnoverDto>();
        public List<MonthlyTurnoverDto> ByMonth { get; set; } = new List<MonthlyTurnoverDto>();
        public List<MonthlyTurnoverDto> MonthlyTerminations { get; set; } = new List<MonthlyTurnoverDto>();
    }

    public class DepartmentTurnoverDto
    {
        public string DepartmentName { get; set; } = "";
        public int Hires { get; set; }
        public int Terminations { get; set; }
        public decimal TurnoverRate { get; set; }
    }

    public class MonthlyTurnoverDto
    {
        public int Month { get; set; }
        public int Hires { get; set; }
        public int Terminations { get; set; }
        public decimal TurnoverRate { get; set; }
    }

    public class MonthlyTerminationDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public int Terminations { get; set; }
        public int TerminationCount { get; set; }
        public decimal TurnoverRate { get; set; }
    }
}
