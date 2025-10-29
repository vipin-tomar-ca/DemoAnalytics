using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class PayrollSummaryReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Department { get; set; } = "";
        public string PayPeriod { get; set; } = "";
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal Taxes { get; set; }
        public DateTime PayDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalTaxes { get; set; }
        public int TotalEmployees { get; set; }
        public decimal AverageGrossPay { get; set; }
        public decimal AverageNetPay { get; set; }
        public List<EmployeePayrollSummaryDto> EmployeeSummaries { get; set; } = new List<EmployeePayrollSummaryDto>();
        public List<DepartmentPayrollSummaryDto> DepartmentSummaries { get; set; } = new List<DepartmentPayrollSummaryDto>();
        public DateTime ReportDate { get; set; }
    }

    public class DepartmentPayrollSummaryDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal AverageGrossPay { get; set; }
        public decimal AverageNetPay { get; set; }
    }
}
