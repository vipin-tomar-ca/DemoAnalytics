using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = "";
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        
        [MaxLength(100)]
        public string Email { get; set; } = "";
        
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        
        [MaxLength(20)]
        public string SocialSecurityNumber { get; set; } = "";
        
        public DateTime? DateOfBirth { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        
        [MaxLength(100)]
        public string Position { get; set; } = "";
        
        [MaxLength(100)]
        public string JobFamily { get; set; } = "";
        
        [MaxLength(50)]
        public string EmploymentType { get; set; } = "Full-time"; // Full-time, Part-time, Contract, etc.
        
        [MaxLength(20)]
        public string EmployeeNumber { get; set; } = "";
        
        [MaxLength(100)]
        public string Province { get; set; } = "";
        
        public int? OrgUnitId { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }
        
        // Collections
        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public ICollection<Absence> Absences { get; set; } = new List<Absence>();
        public ICollection<Compensation> Compensations { get; set; } = new List<Compensation>();
    }
}
