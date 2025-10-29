using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollAnalytics.Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = "";
        
        [Required]
        public string PasswordHash { get; set; } = "";
        
        [MaxLength(50)]
        public string Role { get; set; } = "User";
        
        [MaxLength(100)]
        public string TenantId { get; set; } = "demo-tenant";
    }
}

