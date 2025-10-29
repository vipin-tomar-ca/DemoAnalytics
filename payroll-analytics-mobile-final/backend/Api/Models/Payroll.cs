using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossPay { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTaxes { get; set; }
        
        [Required]
        public DateTime PayPeriodStart { get; set; }
        
        [Required]
        public DateTime PayPeriodEnd { get; set; }
        
        [Required]
        public DateTime PayDate { get; set; }
        
        [MaxLength(20)]
        public string PayFrequency { get; set; } = "Monthly"; // Weekly, Bi-weekly, Monthly, etc.
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal RegularHours { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeHours { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal FederalIncomeTax { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal StateIncomeTax { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal LocalIncomeTax { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SocialSecurityTax { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicareTax { get; set; }
        
        // Navigation properties
        public Employee? Employee { get; set; }
        public ICollection<PayrollItem> PayrollItems { get; set; } = new List<PayrollItem>();
    }
}
