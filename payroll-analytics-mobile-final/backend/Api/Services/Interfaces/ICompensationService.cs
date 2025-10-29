using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollAnalytics.Api.DTOs;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface ICompensationService
    {
        Task<IEnumerable<Compensation>> GetCompensationsAsync(int? employeeId = null, int? departmentId = null);
        Task<Compensation> GetCompensationByIdAsync(int id);
        Task<Compensation> CreateCompensationAsync(Compensation compensation);
        Task<Compensation> UpdateCompensationAsync(Compensation compensation);
        Task<bool> DeleteCompensationAsync(int id);
        Task<CompensationTrendsDto> GetCompensationTrendsAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> CalculateTotalCompensationAsync(int employeeId, DateTime effectiveDate);
    }
}
