using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class AbsenceMetricsDto
    {
        public int Year { get; set; }
        public int TotalAbsences { get; set; }
        public double AverageAbsenceHours { get; set; }
        public double OverallAbsenceRate { get; set; }
        public decimal TotalAbsenceCost { get; set; }
        public decimal AverageAbsenceCost { get; set; }
        public int TotalAbsenceDays { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<AbsenceTypeMetricsDto> ByType { get; set; } = new List<AbsenceTypeMetricsDto>();
        public List<DepartmentAbsenceMetricsDto> ByDepartment { get; set; } = new List<DepartmentAbsenceMetricsDto>();
        public List<MonthlyAbsenceMetricsDto> ByMonth { get; set; } = new List<MonthlyAbsenceMetricsDto>();
        public List<AbsenceTypeMetricsDto> AbsenceTypeMetrics { get; set; } = new List<AbsenceTypeMetricsDto>();
        public List<AbsenceByTypeDto> AbsenceByType { get; set; } = new List<AbsenceByTypeDto>();
        public List<AbsenceByDepartmentDto> AbsenceByDepartment { get; set; } = new List<AbsenceByDepartmentDto>();
        public List<AbsenceByMonthDto> AbsenceByMonth { get; set; } = new List<AbsenceByMonthDto>();
    }

    public class AbsenceTypeMetricsDto
    {
        public int AbsenceTypeId { get; set; }
        public string AbsenceTypeName { get; set; } = "";
        public int Count { get; set; }
        public int Occurrences { get; set; }
        public double TotalHours { get; set; }
        public int TotalDays { get; set; }
    }

    public class DepartmentAbsenceMetricsDto
    {
        public string DepartmentName { get; set; } = "";
        public int Count { get; set; }
        public double TotalHours { get; set; }
    }

    public class MonthlyAbsenceMetricsDto
    {
        public int Month { get; set; }
        public int Count { get; set; }
        public double TotalHours { get; set; }
    }

    public class AbsenceByTypeDto
    {
        public string AbsenceTypeName { get; set; } = "";
        public int Count { get; set; }
        public double TotalHours { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalCost { get; set; }
        public double Percentage { get; set; }
    }

    public class AbsenceByDepartmentDto
    {
        public string DepartmentName { get; set; } = "";
        public int Count { get; set; }
        public double TotalHours { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalCost { get; set; }
        public double AbsenceRate { get; set; }
    }

    public class AbsenceByMonthDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
        public double TotalHours { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalCost { get; set; }
        public double AbsenceRate { get; set; }
    }
}
