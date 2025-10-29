using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.DTOs;
using PayrollAnalytics.Api.Services.Interfaces;

namespace PayrollAnalytics.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly PayrollContext _context;

        public DashboardService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var newHires = await _context.Employees.CountAsync(e => e.HireDate >= DateTime.UtcNow.AddMonths(-1));
            var terminations = await _context.Employees.CountAsync(e => e.TerminationDate >= DateTime.UtcNow.AddMonths(-1));

            var totalPayroll = await _context.Payrolls.SumAsync(p => p.GrossPay);
            var averageSalary = await _context.Compensations.AverageAsync(c => c.BaseSalary);

            var overtimeCost = await _context.Payrolls.SumAsync(p => p.OvertimeHours * 25); // Assuming $25 per hour
            var absenceCost = await _context.Absences.SumAsync(a => a.Hours * 25); // Assuming $25 per hour

            var turnoverRate = totalEmployees > 0 ? (double)terminations / totalEmployees * 100 : 0;
            var absenceRate = activeEmployees > 0 ? (double)await _context.Absences.CountAsync() / activeEmployees * 100 : 0;

            return new DashboardSummaryDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                NewHires = newHires,
                Terminations = terminations,
                TotalPayroll = totalPayroll,
                AverageSalary = averageSalary,
                OvertimeCost = overtimeCost,
                AbsenceCost = absenceCost,
                TurnoverRate = turnoverRate,
                AbsenceRate = absenceRate,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<DepartmentStatsDto>> GetDepartmentStatsAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();

            var stats = new List<DepartmentStatsDto>();

            foreach (var dept in departments)
            {
                var employees = dept.Employees.Where(e => e.IsActive).ToList();
                var employeeIds = employees.Select(e => e.Id).ToList();

                var totalPayroll = await _context.Payrolls
                    .Where(p => employeeIds.Contains(p.EmployeeId))
                    .SumAsync(p => p.GrossPay);

                var averageSalary = await _context.Compensations
                    .Where(c => employeeIds.Contains(c.EmployeeId))
                    .AverageAsync(c => c.BaseSalary);

                var overtimeCost = await _context.Payrolls
                    .Where(p => employeeIds.Contains(p.EmployeeId))
                    .SumAsync(p => p.OvertimeHours * 25);

                var absenceCost = await _context.Absences
                    .Where(a => employeeIds.Contains(a.EmployeeId))
                    .SumAsync(a => a.Hours * 25);

                var turnoverRate = employees.Count > 0 ? 
                    (double)employees.Count(e => e.TerminationDate >= DateTime.UtcNow.AddMonths(-1)) / employees.Count * 100 : 0;

                var absenceRate = employees.Count > 0 ? 
                    (double)await _context.Absences.CountAsync(a => employeeIds.Contains(a.EmployeeId)) / employees.Count * 100 : 0;

                stats.Add(new DepartmentStatsDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    EmployeeCount = employees.Count,
                    TotalPayroll = totalPayroll,
                    AverageSalary = averageSalary,
                    OvertimeCost = overtimeCost,
                    AbsenceCost = absenceCost,
                    TurnoverRate = turnoverRate,
                    AbsenceRate = absenceRate,
                    LastUpdated = DateTime.UtcNow
                });
            }

            return stats;
        }

        public async Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var totalExits = await _context.Employees
                .CountAsync(e => e.TerminationDate >= start && e.TerminationDate <= end);

            var voluntaryExits = await _context.Employees
                .CountAsync(e => e.TerminationDate >= start && e.TerminationDate <= end && e.EmploymentType == "Voluntary");

            var involuntaryExits = totalExits - voluntaryExits;

            var totalEmployees = await _context.Employees.CountAsync();
            var overallTurnoverRate = totalEmployees > 0 ? (decimal)totalExits / totalEmployees * 100 : 0;
            var voluntaryTurnoverRate = totalEmployees > 0 ? (decimal)voluntaryExits / totalEmployees * 100 : 0;
            var involuntaryTurnoverRate = totalEmployees > 0 ? (decimal)involuntaryExits / totalEmployees * 100 : 0;

            var turnoverCost = totalExits * 50000; // Assuming $50,000 cost per turnover

            return new TurnoverMetricsDto
            {
                OverallTurnoverRate = overallTurnoverRate,
                VoluntaryTurnoverRate = voluntaryTurnoverRate,
                InvoluntaryTurnoverRate = involuntaryTurnoverRate,
                TotalExits = totalExits,
                VoluntaryExits = voluntaryExits,
                InvoluntaryExits = involuntaryExits,
                TurnoverCost = turnoverCost,
                ByDepartment = await GetTurnoverByDepartmentAsync(start, end),
                ByMonth = await GetTurnoverByMonthAsync(start, end),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<CompensationTrendsDto> GetCompensationTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var compensations = await _context.Compensations
                .Include(c => c.Employee)
                .Include(c => c.PayGrade)
                .Where(c => c.EffectiveDate >= start && c.EffectiveDate <= end)
                .ToListAsync();

            var salaries = compensations.Select(c => c.BaseSalary).ToList();

            return new CompensationTrendsDto
            {
                AverageSalary = salaries.Any() ? salaries.Average() : 0,
                MedianSalary = salaries.Any() ? GetMedian(salaries) : 0,
                MinSalary = salaries.Any() ? salaries.Min() : 0,
                MaxSalary = salaries.Any() ? salaries.Max() : 0,
                SalaryVariance = salaries.Any() ? CalculateVariance(salaries) : 0,
                ByDepartment = compensations.GroupBy(c => c.Employee.Department.Name)
                    .Select(g => new DepartmentCompensationDto
                    {
                        DepartmentName = g.Key,
                        AverageSalary = g.Average(c => c.BaseSalary),
                        TotalCompensation = g.Sum(c => c.TotalCompensation)
                    }).ToList(),
                ByMonth = compensations.GroupBy(c => new { c.EffectiveDate.Year, c.EffectiveDate.Month })
                    .Select(g => new MonthlyCompensationDto
                    {
                        Month = g.Key.Month,
                        AverageSalary = g.Average(c => c.BaseSalary),
                        TotalCompensation = g.Sum(c => c.TotalCompensation)
                    }).ToList(),
                ByPayGrade = compensations.GroupBy(c => c.PayGrade.Name)
                    .Select(g => new PayGradeCompensationDto
                    {
                        PayGradeName = g.Key,
                        AverageSalary = g.Average(c => c.BaseSalary),
                        TotalCompensation = g.Sum(c => c.TotalCompensation)
                    }).ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<AbsenceMetricsDto> GetAbsenceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
            var end = endDate ?? DateTime.UtcNow;

            var absences = await _context.Absences
                .Include(a => a.Employee)
                .Include(a => a.AbsenceType)
                .Where(a => a.StartDate >= start && a.StartDate <= end)
                .ToListAsync();

            var totalAbsenceDays = absences.Sum(a => (int)a.Hours / 8);
            var totalAbsenceCost = absences.Sum(a => a.Hours * 25);
            var uniqueEmployees = absences.Select(a => a.EmployeeId).Distinct().Count();

            return new AbsenceMetricsDto
            {
                OverallAbsenceRate = uniqueEmployees > 0 ? (double)absences.Count / uniqueEmployees : 0,
                TotalAbsenceCost = totalAbsenceCost,
                AverageAbsenceCost = totalAbsenceDays > 0 ? totalAbsenceCost / totalAbsenceDays : 0,
                TotalAbsenceDays = totalAbsenceDays,
                ByType = absences.GroupBy(a => a.AbsenceType.Name)
                    .Select(g => new AbsenceTypeMetricsDto
                    {
                        AbsenceTypeName = g.Key,
                        Count = g.Count(),
                        TotalHours = (double)g.Sum(a => a.Hours)
                    }).ToList(),
                ByDepartment = absences.GroupBy(a => a.Employee.Department.Name)
                    .Select(g => new DepartmentAbsenceMetricsDto
                    {
                        DepartmentName = g.Key,
                        Count = g.Count(),
                        TotalHours = (double)g.Sum(a => a.Hours)
                    }).ToList(),
                ByMonth = absences.GroupBy(a => new { a.StartDate.Year, a.StartDate.Month })
                    .Select(g => new MonthlyAbsenceMetricsDto
                    {
                        Month = g.Key.Month,
                        Count = g.Count(),
                        TotalHours = (double)g.Sum(a => a.Hours)
                    }).ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }

        private async Task<List<DepartmentTurnoverDto>> GetTurnoverByDepartmentAsync(DateTime start, DateTime end)
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();

            var result = new List<DepartmentTurnoverDto>();

            foreach (var dept in departments)
            {
                var totalEmployees = dept.Employees.Count;
                var exits = dept.Employees.Count(e => e.TerminationDate >= start && e.TerminationDate <= end);
                var turnoverRate = totalEmployees > 0 ? (decimal)exits / totalEmployees * 100 : 0;

                result.Add(new DepartmentTurnoverDto
                {
                    DepartmentName = dept.Name,
                    Hires = 0, // This would need to be calculated based on hire dates
                    Terminations = exits,
                    TurnoverRate = turnoverRate
                });
            }

            return result;
        }

        private async Task<List<MonthlyTurnoverDto>> GetTurnoverByMonthAsync(DateTime start, DateTime end)
        {
            var exits = await _context.Employees
                .Where(e => e.TerminationDate >= start && e.TerminationDate <= end)
                .GroupBy(e => new { e.TerminationDate.Value.Year, e.TerminationDate.Value.Month })
                .Select(g => new MonthlyTurnoverDto
                {
                    Month = g.Key.Month,
                    Hires = 0, // This would need to be calculated based on hire dates
                    Terminations = g.Count(),
                    TurnoverRate = 0 // This would need total employees per month to calculate properly
                })
                .ToListAsync();

            return exits;
        }

        private decimal GetMedian(List<decimal> values)
        {
            if (!values.Any()) return 0;
            
            var sortedValues = values.OrderBy(x => x).ToList();
            int count = sortedValues.Count;
            
            if (count % 2 == 0)
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2;
            else
                return sortedValues[count / 2];
        }

        private decimal CalculateVariance(List<decimal> values)
        {
            if (!values.Any()) return 0;
            
            var mean = values.Average();
            var variance = values.Sum(x => (x - mean) * (x - mean)) / values.Count;
            return variance;
        }
    }
}
