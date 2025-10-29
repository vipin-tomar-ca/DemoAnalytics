using System;

namespace PayrollAnalytics.Api.DTOs
{
    public class CompensationDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public decimal Benefits { get; set; }
        public decimal TotalCompensation { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string PayGrade { get; set; } = "";
    }
}
