using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class PayrollItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PayrollId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ItemType { get; set; } = "Earning"; // Earning, Deduction, Tax, etc.
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string Description { get; set; } = "";
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [MaxLength(50)]
        public string RateType { get; set; } = "Fixed"; // Percentage, Fixed, etc.
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Rate { get; set; }
        
        public double? Quantity { get; set; }
        
        // Navigation property
        public Payroll? Payroll { get; set; }
    }
}
