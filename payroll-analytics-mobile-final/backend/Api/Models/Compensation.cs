using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class Compensation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public int PayGradeId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Commission { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Benefits { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCompensation { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PayrollTaxes { get; set; }
        
        [Required]
        public DateTime EffectiveDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [MaxLength(50)]
        public string Currency { get; set; } = "USD";
        
        [MaxLength(20)]
        public string PayFrequency { get; set; } = "Monthly";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        public Employee? Employee { get; set; }
        public PayGrade? PayGrade { get; set; }
    }
}
