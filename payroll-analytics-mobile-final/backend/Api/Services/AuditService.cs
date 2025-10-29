using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.Services.Interfaces;

namespace PayrollAnalytics.Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly PayrollContext _context;

        public AuditService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityName = null, int? entityId = null, string userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<AuditLog> GetAuditLogByIdAsync(int id)
        {
            return await _context.AuditLogs.FindAsync(id);
        }

        public async Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task LogEntityChangeAsync(string entityName, int entityId, string action, string userId, string oldValues = null, string newValues = null, string description = null)
        {
            var auditLog = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                UserId = userId,
                OldValues = oldValues,
                NewValues = newValues,
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            await CreateAuditLogAsync(auditLog);
        }
    }
}
