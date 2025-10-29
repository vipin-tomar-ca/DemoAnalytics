using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.DTOs;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.Services.Interfaces;

namespace PayrollAnalytics.Api.Services
{
    public class CompensationService : ICompensationService
    {
        private readonly PayrollContext _context;

        public CompensationService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Compensation>> GetCompensationsAsync(int? employeeId = null, int? departmentId = null)
        {
            var query = _context.Compensations
                .Include(c => c.Employee)
                .Include(c => c.PayGrade)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            if (departmentId.HasValue)
                query = query.Where(c => c.Employee.DepartmentId == departmentId.Value);

            return await query.ToListAsync();
        }

        public async Task<Compensation> GetCompensationByIdAsync(int id)
        {
            return await _context.Compensations
                .Include(c => c.Employee)
                .Include(c => c.PayGrade)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Compensation> CreateCompensationAsync(Compensation compensation)
        {
            _context.Compensations.Add(compensation);
            await _context.SaveChangesAsync();
            return compensation;
        }

        public async Task<Compensation> UpdateCompensationAsync(Compensation compensation)
        {
            _context.Compensations.Update(compensation);
            await _context.SaveChangesAsync();
            return compensation;
        }

        public async Task<bool> DeleteCompensationAsync(int id)
        {
            var compensation = await _context.Compensations.FindAsync(id);
            if (compensation == null) return false;

            _context.Compensations.Remove(compensation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CompensationTrendsDto> GetCompensationTrendsAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Compensations
                .Include(c => c.Employee)
                .Include(c => c.PayGrade)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(c => c.Employee.DepartmentId == departmentId.Value);

            if (startDate.HasValue)
                query = query.Where(c => c.EffectiveDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EffectiveDate <= endDate.Value);

            var compensations = await query.ToListAsync();

            var salaries = compensations.Select(c => c.BaseSalary).ToList();

            var trends = new CompensationTrendsDto
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

            return trends;
        }

        public async Task<decimal> CalculateTotalCompensationAsync(int employeeId, DateTime effectiveDate)
        {
            var compensation = await _context.Compensations
                .Where(c => c.EmployeeId == employeeId && c.EffectiveDate <= effectiveDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync();

            return compensation?.TotalCompensation ?? 0;
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
