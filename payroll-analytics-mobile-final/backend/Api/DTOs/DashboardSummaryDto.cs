using System;

namespace PayrollAnalytics.Api.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int NewHires { get; set; }
        public int Terminations { get; set; }
        public decimal TotalPayroll { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal OvertimeCost { get; set; }
        public decimal AbsenceCost { get; set; }
        public double TurnoverRate { get; set; }
        public double AbsenceRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
