using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class CompensationTrendsDto
    {
        public int Year { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal MedianSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal SalaryVariance { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<MonthlyCompensationDto> ByMonth { get; set; } = new List<MonthlyCompensationDto>();
        public List<DepartmentCompensationDto> ByDepartment { get; set; } = new List<DepartmentCompensationDto>();
        public List<PayGradeCompensationDto> ByPayGrade { get; set; } = new List<PayGradeCompensationDto>();
        public List<MonthlyCompensationDto> MonthlyData { get; set; } = new List<MonthlyCompensationDto>();
        public List<CompensationByDepartmentDto> CompensationByDepartment { get; set; } = new List<CompensationByDepartmentDto>();
        public List<CompensationByMonthDto> CompensationByMonth { get; set; } = new List<CompensationByMonthDto>();
        public List<CompensationByPayGradeDto> CompensationByPayGrade { get; set; } = new List<CompensationByPayGradeDto>();
    }

    public class MonthlyCompensationDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = "";
        public decimal AverageSalary { get; set; }
        public decimal TotalCompensation { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class DepartmentCompensationDto
    {
        public string DepartmentName { get; set; } = "";
        public decimal AverageSalary { get; set; }
        public decimal TotalCompensation { get; set; }
    }

    public class PayGradeCompensationDto
    {
        public string PayGradeName { get; set; } = "";
        public decimal AverageSalary { get; set; }
        public decimal TotalCompensation { get; set; }
    }

    public class CompensationByDepartmentDto
    {
        public string DepartmentName { get; set; } = "";
        public decimal AverageSalary { get; set; }
        public decimal MedianSalary { get; set; }
        public decimal TotalCompensation { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class CompensationByMonthDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal TotalCompensation { get; set; }
        public decimal TotalPayroll { get; set; }
    }

    public class CompensationByPayGradeDto
    {
        public string PayGradeName { get; set; } = "";
        public decimal AverageSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal TotalCompensation { get; set; }
        public int EmployeeCount { get; set; }
    }
}
