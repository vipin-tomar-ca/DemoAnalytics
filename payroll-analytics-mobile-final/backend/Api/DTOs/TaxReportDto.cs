using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class TaxReportDto
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
        public decimal TotalWages { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal TotalFederalIncomeTax { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal TotalStateIncomeTax { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
        public decimal TotalLocalIncomeTax { get; set; }
        public decimal TotalSocialSecurityTax { get; set; }
        public decimal TotalMedicareTax { get; set; }
        public decimal TotalUnemploymentTax { get; set; }
        public decimal TotalTaxes { get; set; }
        public List<EmployeeTaxSummaryDto> EmployeeTaxSummaries { get; set; } = new List<EmployeeTaxSummaryDto>();
        public List<DepartmentTaxSummaryDto> ByDepartment { get; set; } = new List<DepartmentTaxSummaryDto>();
        public List<MonthlyTaxSummaryDto> ByMonth { get; set; } = new List<MonthlyTaxSummaryDto>();
        public List<TaxByDepartmentDto> TaxByDepartment { get; set; } = new List<TaxByDepartmentDto>();
        public List<TaxByMonthDto> TaxByMonth { get; set; } = new List<TaxByMonthDto>();
        public List<TaxByEmployeeDto> ByEmployee { get; set; } = new List<TaxByEmployeeDto>();
    }

    public class EmployeeTaxSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string SocialSecurityNumber { get; set; } = "";
        public decimal Wages { get; set; }
        public decimal TotalWages { get; set; }
        public decimal FederalTaxWithheld { get; set; }
        public decimal FederalIncomeTax { get; set; }
        public decimal SocialSecurityTax { get; set; }
        public decimal MedicareTax { get; set; }
        public decimal StateTaxWithheld { get; set; }
        public decimal StateIncomeTax { get; set; }
        public decimal LocalTaxWithheld { get; set; }
        public decimal LocalIncomeTax { get; set; }
    }

    public class DepartmentTaxSummaryDto
    {
        public string DepartmentName { get; set; } = "";
        public decimal TotalWages { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
    }

    public class MonthlyTaxSummaryDto
    {
        public int Month { get; set; }
        public decimal TotalWages { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
    }

    public class TaxByDepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public decimal TotalWages { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal TotalFederalIncomeTax { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal TotalStateIncomeTax { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
        public decimal TotalLocalIncomeTax { get; set; }
        public decimal TotalSocialSecurityTax { get; set; }
        public decimal TotalMedicareTax { get; set; }
        public decimal TotalTaxes { get; set; }
    }

    public class TaxByMonthDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalWages { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal TotalFederalIncomeTax { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal TotalStateIncomeTax { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
        public decimal TotalLocalIncomeTax { get; set; }
        public decimal TotalSocialSecurityTax { get; set; }
        public decimal TotalMedicareTax { get; set; }
        public decimal TotalTaxes { get; set; }
    }

    public class TaxByEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public decimal TotalWages { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalFederalTaxWithheld { get; set; }
        public decimal FederalIncomeTax { get; set; }
        public decimal TotalStateTaxWithheld { get; set; }
        public decimal StateIncomeTax { get; set; }
        public decimal TotalLocalTaxWithheld { get; set; }
        public decimal LocalIncomeTax { get; set; }
        public decimal SocialSecurityTax { get; set; }
        public decimal MedicareTax { get; set; }
        public decimal TotalTaxes { get; set; }
    }
}
