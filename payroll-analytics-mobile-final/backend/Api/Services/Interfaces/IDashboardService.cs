using System;
using System.Threading.Tasks;
using PayrollAnalytics.Api.DTOs;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<IEnumerable<DepartmentStatsDto>> GetDepartmentStatsAsync();
        Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<CompensationTrendsDto> GetCompensationTrendsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<AbsenceMetricsDto> GetAbsenceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
