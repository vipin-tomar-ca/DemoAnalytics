using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class PayGrade
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string Description { get; set; } = "";
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }
        
        // Navigation property
        public ICollection<Compensation> Compensations { get; set; } = new List<Compensation>();
    }
}
