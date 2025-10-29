using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollAnalytics.Api.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = "";
        
        [Required]
        public int EntityId { get; set; }
        
        [MaxLength(100)]
        public string TableName { get; set; } = "";
        
        [MaxLength(100)]
        public string PrimaryKey { get; set; } = "";
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = "Create"; // Create, Update, Delete
        
        [MaxLength(100)]
        public string UserId { get; set; } = "";
        
        [MaxLength(100)]
        public string UserName { get; set; } = "";
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string OldValues { get; set; } = "";
        
        [MaxLength(500)]
        public string NewValues { get; set; } = "";
        
        [MaxLength(1000)]
        public string Description { get; set; } = "";
        
        [MaxLength(50)]
        public string IpAddress { get; set; } = "";
        
        [MaxLength(500)]
        public string UserAgent { get; set; } = "";
    }
}
