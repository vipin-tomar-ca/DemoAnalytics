using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string Address { get; set; } = "";
        
        [MaxLength(100)]
        public string City { get; set; } = "";
        
        [MaxLength(100)]
        public string State { get; set; } = "";
        
        [MaxLength(20)]
        public string PostalCode { get; set; } = "";
        
        [MaxLength(100)]
        public string Country { get; set; } = "";
        
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        public bool IsActive { get; set; } = true;

        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
        
        // Navigation property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
