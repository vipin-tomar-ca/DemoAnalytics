using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class AbsenceType
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string Description { get; set; } = "";
        
        public bool IsPaid { get; set; }
        public bool RequiresApproval { get; set; }
        
        // Navigation property
        public ICollection<Absence> Absences { get; set; } = new List<Absence>();
    }
}
