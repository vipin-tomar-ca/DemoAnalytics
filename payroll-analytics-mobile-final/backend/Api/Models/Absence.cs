using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class Absence
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public int AbsenceTypeId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Hours { get; set; }
        
        public bool IsOvertime { get; set; }
        
        public DateTime Date { get; set; }
        
        [MaxLength(50)]
        public string Type { get; set; } = "";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        
        [MaxLength(500)]
        public string Reason { get; set; } = "";
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public bool IsPaid { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        public Employee? Employee { get; set; }
        public AbsenceType? AbsenceType { get; set; }
    }
}
