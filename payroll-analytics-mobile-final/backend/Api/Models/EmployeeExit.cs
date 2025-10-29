using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class EmployeeExit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public DateTime ExitDate { get; set; }
        
        [MaxLength(100)]
        public string Reason { get; set; } = "";
        
        [MaxLength(50)]
        public string Type { get; set; } = "Voluntary"; // Voluntary, Involuntary
        
        [MaxLength(500)]
        public string Notes { get; set; } = "";
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Employee? Employee { get; set; }
    }
}
