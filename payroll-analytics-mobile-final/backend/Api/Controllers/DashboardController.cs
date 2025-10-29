using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollAnalytics.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    // [Authorize] // Commented out to disable authentication
    public class DashboardController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(PayrollContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
        {
            try
            {
                var totalEmployees = await _context.Employees.CountAsync();
                var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
                var totalDepartments = await _context.Departments.CountAsync();
                
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                
                var payrollCost = await _context.Compensations
                    .Where(c => c.EffectiveDate.Year == currentYear && c.EffectiveDate.Month == currentMonth)
                    .SumAsync(c => c.BaseSalary);

                var summary = new DashboardSummaryDto
                {
                    TotalEmployees = totalEmployees,
                    ActiveEmployees = activeEmployees,
                    TotalPayroll = payrollCost,
                    AverageSalary = totalEmployees > 0 ? payrollCost / totalEmployees : 0,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, "Internal server error while retrieving dashboard summary");
            }
        }

        [HttpGet("department-stats")]
        public async Task<ActionResult<IEnumerable<DepartmentStatsDto>>> GetDepartmentStats()
        {
            try
            {
                var stats = await _context.Departments
                    .Select(d => new DepartmentStatsDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name,
                        EmployeeCount = d.Employees.Count,
                        AverageSalary = d.Employees
                            .SelectMany(e => e.Compensations)
                            .OrderByDescending(c => c.EffectiveDate)
                            .GroupBy(c => c.EmployeeId)
                            .Select(g => g.First())
                            .Average(c => c.BaseSalary)
                    })
                    .OrderByDescending(d => d.EmployeeCount)
                    .ToListAsync();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department statistics");
                return StatusCode(500, "Internal server error while retrieving department statistics");
            }
        }

        [HttpGet("turnover-metrics")]
        public async Task<ActionResult<TurnoverMetricsDto>> GetTurnoverMetrics([FromQuery] int? year = null)
        {
            try
            {
                var currentYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(currentYear, 1, 1);
                var endDate = startDate.AddYears(1);

                var terminations = await _context.Employees
                    .Where(e => e.TerminationDate.HasValue && 
                              e.TerminationDate >= startDate && 
                              e.TerminationDate < endDate)
                    .GroupBy(e => e.TerminationDate.Value.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Month)
                    .ToListAsync();

                var metrics = new TurnoverMetricsDto
                {
                    Year = currentYear,
                    ByMonth = Enumerable.Range(1, 12)
                        .Select(month => new MonthlyTurnoverDto
                        {
                            Month = month,
                            Hires = 0, // This would need to be calculated based on hire dates
                            Terminations = terminations.FirstOrDefault(t => t.Month == month)?.Count ?? 0,
                            TurnoverRate = 0 // This would need total employees per month to calculate properly
                        })
                        .ToList()
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting turnover metrics");
                return StatusCode(500, "Internal server error while retrieving turnover metrics");
            }
        }

        [HttpGet("compensation-trends")]
        public async Task<ActionResult<CompensationTrendsDto>> GetCompensationTrends([FromQuery] int months = 12)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                var trends = await _context.Compensations
                    .Where(c => c.EffectiveDate >= startDate && c.EffectiveDate <= endDate)
                    .GroupBy(c => new { c.EffectiveDate.Year, c.EffectiveDate.Month })
                    .Select(g => new MonthlyCompensationDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        AverageSalary = g.Average(c => c.BaseSalary),
                        EmployeeCount = g.Select(c => c.EmployeeId).Distinct().Count()
                    })
                    .OrderBy(t => t.Year)
                    .ThenBy(t => t.Month)
                    .ToListAsync();

                return Ok(new CompensationTrendsDto { MonthlyData = trends });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compensation trends");
                return StatusCode(500, "Internal server error while retrieving compensation trends");
            }
        }

        [HttpGet("absence-metrics")]
        public async Task<ActionResult<AbsenceMetricsDto>> GetAbsenceMetrics([FromQuery] int? year = null)
        {
            try
            {
                var currentYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(currentYear, 1, 1);
                var endDate = startDate.AddYears(1);

                var absences = await _context.Absences
                    .Where(a => a.StartDate < endDate && a.EndDate >= startDate)
                    .GroupBy(a => a.AbsenceTypeId)
                    .Select(g => new AbsenceTypeMetricsDto
                    {
                        AbsenceTypeId = g.Key,
                        AbsenceTypeName = g.First().AbsenceType.Name,
                        TotalDays = g.Sum(a => ((a.EndDate ?? a.StartDate) - a.StartDate).Days + 1),
                        Occurrences = g.Count()
                    })
                    .OrderByDescending(a => a.TotalDays)
                    .ToListAsync();

                var metrics = new AbsenceMetricsDto
                {
                    Year = currentYear,
                    AbsenceTypeMetrics = absences
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting absence metrics");
                return StatusCode(500, "Internal server error while retrieving absence metrics");
            }
        }
    }
}
