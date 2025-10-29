using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollAnalytics.Api.Models
{
    public class OrgUnit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";
    }
}

