using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollAnalytics.Api.DTOs;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface IAbsenceService
    {
        Task<IEnumerable<Absence>> GetAbsencesAsync(int? employeeId = null, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Absence> GetAbsenceByIdAsync(int id);
        Task<Absence> CreateAbsenceAsync(Absence absence);
        Task<Absence> UpdateAbsenceAsync(Absence absence);
        Task<bool> DeleteAbsenceAsync(int id);
        Task<AbsenceMetricsDto> GetAbsenceMetricsAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> CalculateAbsenceCostAsync(int employeeId, DateTime startDate, DateTime endDate);
    }
}
