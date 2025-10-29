using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollAnalytics.Api.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string Description { get; set; } = "";
        
        // Navigation property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
