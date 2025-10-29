using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollAnalytics.Api.DTOs;

namespace PayrollAnalytics.Api.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<EmployeeEarningsDto>> GetEmployeeEarningsReportAsync(int? employeeId = null, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<DepartmentPayrollReportDto>> GetDepartmentPayrollReportAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<TaxReportDto> GetTaxReportAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> GeneratePayrollReportAsync(string reportType, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
