using System;
using System.Collections.Generic;

namespace PayrollAnalytics.Api.DTOs
{
    public class EmployeeEarningsDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string Position { get; set; } = "";
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public decimal Benefits { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalTaxes { get; set; }
        public double RegularHours { get; set; }
        public double OvertimeHours { get; set; }
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public DateTime PayDate { get; set; }
        public List<PayrollItemDto> PayrollItems { get; set; } = new List<PayrollItemDto>();
    }

    public class PayrollItemDto
    {
        public string ItemType { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string RateType { get; set; } = "";
        public decimal? Rate { get; set; }
        public double? Quantity { get; set; }
    }
}
