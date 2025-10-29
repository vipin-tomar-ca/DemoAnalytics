using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityName = null, int? entityId = null, string userId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<AuditLog> GetAuditLogByIdAsync(int id);
        Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog);
        Task LogEntityChangeAsync(string entityName, int entityId, string action, string userId, string oldValues = null, string newValues = null, string description = null);
    }
}
