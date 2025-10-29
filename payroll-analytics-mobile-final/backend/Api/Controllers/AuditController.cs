using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollAnalytics.Api.Controllers
{
    // [Authorize(Roles = "Admin,Auditor")] // Commented out to disable authentication
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<AuditController> _logger;

        public AuditController(PayrollContext context, ILogger<AuditController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<PaginatedResult<AuditLog>>> GetAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string userId = null,
            [FromQuery] string tableName = null,
            [FromQuery] string action = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(l => l.UserId == userId);
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    query = query.Where(l => l.TableName == tableName);
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(l => l.Action == action);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp <= endDate.Value);
                }

                var totalRecords = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResult<AuditLog>
                {
                    Items = logs,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, "Internal server error while retrieving audit logs");
            }
        }

        [HttpGet("logs/{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
        {
            try
            {
                var log = await _context.AuditLogs.FindAsync(id);

                if (log == null)
                {
                    return NotFound();
                }

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit log with ID {id}");
                return StatusCode(500, "Internal server error while retrieving audit log");
            }
        }

        [HttpGet("changes/{entityType}/{entityId}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetEntityAuditLogs(
            string entityType, 
            string entityId,
            [FromQuery] int? limit = null)
        {
            try
            {
                var query = _context.AuditLogs
                    .Where(l => l.TableName == entityType && l.PrimaryKey == entityId)
                    .OrderByDescending(l => l.Timestamp);

                if (limit.HasValue)
                {
                    query = (IOrderedQueryable<AuditLog>)query.Take(limit.Value);
                }

                var logs = await query.ToListAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit logs for {entityType} with ID {entityId}");
                return StatusCode(500, "Internal server error while retrieving entity audit logs");
            }
        }

        [HttpGet("user-activity/{userId}")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetUserActivity(
            string userId,
            [FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                
                var activities = await _context.AuditLogs
                    .Where(l => l.UserId == userId && l.Timestamp >= startDate)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activity for user {userId}");
                return StatusCode(500, "Internal server error while retrieving user activity");
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<AuditSummaryDto>> GetAuditSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var summary = new AuditSummaryDto
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalActions = await _context.AuditLogs
                        .CountAsync(l => l.Timestamp >= startDate && l.Timestamp <= endDate),
                    ActionsByType = await _context.AuditLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                        .GroupBy(l => l.Action)
                        .Select(g => new ActionCountDto
                        {
                            Action = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync(),
                    ActionsByUser = await _context.AuditLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                        .GroupBy(l => l.UserId)
                        .Select(g => new UserActionCountDto
                        {
                            UserId = g.Key,
                            UserName = g.First().UserName,
                            Count = g.Count()
                        })
                        .OrderByDescending(u => u.Count)
                        .Take(10)
                        .ToListAsync(),
                    ActionsByTable = await _context.AuditLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                        .GroupBy(l => l.TableName)
                        .Select(g => new TableActionCountDto
                        {
                            TableName = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(t => t.Count)
                        .ToListAsync()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit summary");
                return StatusCode(500, "Internal server error while generating audit summary");
            }
        }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class AuditSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalActions { get; set; }
        public List<ActionCountDto> ActionsByType { get; set; }
        public List<UserActionCountDto> ActionsByUser { get; set; }
        public List<TableActionCountDto> ActionsByTable { get; set; }
    }

    public class ActionCountDto
    {
        public string Action { get; set; }
        public int Count { get; set; }
    }

    public class UserActionCountDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }
    }

    public class TableActionCountDto
    {
        public string TableName { get; set; }
        public int Count { get; set; }
    }
}
