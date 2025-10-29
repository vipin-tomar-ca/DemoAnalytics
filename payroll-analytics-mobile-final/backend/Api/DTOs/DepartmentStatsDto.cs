using System;

namespace PayrollAnalytics.Api.DTOs
{
    public class DepartmentStatsDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalPayroll { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal OvertimeCost { get; set; }
        public decimal AbsenceCost { get; set; }
        public double TurnoverRate { get; set; }
        public double AbsenceRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
