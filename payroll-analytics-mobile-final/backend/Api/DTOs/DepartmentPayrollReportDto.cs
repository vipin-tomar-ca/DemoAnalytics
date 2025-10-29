using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class DepartmentPayrollReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal AverageGrossPay { get; set; }
        public decimal AverageNetPay { get; set; }
        public decimal OvertimeCost { get; set; }
        public decimal BenefitsCost { get; set; }
        public string ReportPeriod { get; set; } = "";
        public int TotalEmployees { get; set; }
        public decimal TotalPayrollCost { get; set; }
        public List<EmployeePayrollSummaryDto> EmployeeSummaries { get; set; } = new List<EmployeePayrollSummaryDto>();
        public List<EmployeePayrollSummaryDto> EmployeePayrolls { get; set; } = new List<EmployeePayrollSummaryDto>();
        public DateTime ReportDate { get; set; }
    }

    public class EmployeePayrollSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Position { get; set; } = "";
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal Taxes { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Benefits { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AveragePay { get; set; }
        public DateTime LastPayDate { get; set; }
    }
}
