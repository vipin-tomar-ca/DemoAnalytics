using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.Services.Interfaces;

namespace PayrollAnalytics.Api.Services
{
    public class LookupService : ILookupService
    {
        private readonly PayrollContext _context;

        public LookupService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<IEnumerable<PayGrade>> GetPayGradesAsync()
        {
            return await _context.PayGrades.ToListAsync();
        }

        public async Task<IEnumerable<AbsenceType>> GetAbsenceTypesAsync()
        {
            return await _context.AbsenceTypes.ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync()
        {
            return await _context.Locations.ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task<PayGrade> GetPayGradeByIdAsync(int id)
        {
            return await _context.PayGrades.FindAsync(id);
        }

        public async Task<AbsenceType> GetAbsenceTypeByIdAsync(int id)
        {
            return await _context.AbsenceTypes.FindAsync(id);
        }

        public async Task<Location> GetLocationByIdAsync(int id)
        {
            return await _context.Locations.FindAsync(id);
        }
    }
}
