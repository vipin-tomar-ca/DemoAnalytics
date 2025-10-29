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
    public class AbsenceService : IAbsenceService
    {
        private readonly PayrollContext _context;

        public AbsenceService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Absence>> GetAbsencesAsync(int? employeeId = null, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Absences
                .Include(a => a.Employee)
                .Include(a => a.AbsenceType)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.StartDate <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<Absence> GetAbsenceByIdAsync(int id)
        {
            return await _context.Absences
                .Include(a => a.Employee)
                .Include(a => a.AbsenceType)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Absence> CreateAbsenceAsync(Absence absence)
        {
            _context.Absences.Add(absence);
            await _context.SaveChangesAsync();
            return absence;
        }

        public async Task<Absence> UpdateAbsenceAsync(Absence absence)
        {
            _context.Absences.Update(absence);
            await _context.SaveChangesAsync();
            return absence;
        }

        public async Task<bool> DeleteAbsenceAsync(int id)
        {
            var absence = await _context.Absences.FindAsync(id);
            if (absence == null) return false;

            _context.Absences.Remove(absence);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AbsenceMetricsDto> GetAbsenceMetricsAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Absences
                .Include(a => a.Employee)
                .Include(a => a.AbsenceType)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.StartDate <= endDate.Value);

            var absences = await query.ToListAsync();

            var metrics = new AbsenceMetricsDto
            {
                TotalAbsenceDays = absences.Sum(a => (int)a.Hours / 8), // Assuming 8 hours per day
                TotalAbsenceCost = absences.Sum(a => a.Hours * 25), // Assuming $25 per hour
                OverallAbsenceRate = absences.Count > 0 ? (double)absences.Count / absences.Select(a => a.EmployeeId).Distinct().Count() : 0,
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
                LastUpdated = DateTime.UtcNow
            };

            metrics.AverageAbsenceCost = metrics.TotalAbsenceDays > 0 ? metrics.TotalAbsenceCost / metrics.TotalAbsenceDays : 0;

            return metrics;
        }

        public async Task<decimal> CalculateAbsenceCostAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var absences = await _context.Absences
                .Where(a => a.EmployeeId == employeeId && a.StartDate >= startDate && a.StartDate <= endDate)
                .ToListAsync();

            return absences.Sum(a => a.Hours * 25); // Assuming $25 per hour
        }
    }
}
