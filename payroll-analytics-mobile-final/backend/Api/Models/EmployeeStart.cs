using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class EmployeeStart
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [MaxLength(100)]
        public string Position { get; set; } = "";
        
        [MaxLength(100)]
        public string Department { get; set; } = "";
        
        [MaxLength(500)]
        public string Reason { get; set; } = "";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Employee? Employee { get; set; }
    }
}
