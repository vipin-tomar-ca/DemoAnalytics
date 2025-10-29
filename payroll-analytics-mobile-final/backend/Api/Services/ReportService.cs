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
    public class ReportService : IReportService
    {
        private readonly PayrollContext _context;

        public ReportService(PayrollContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeEarningsDto>> GetEmployeeEarningsReportAsync(int? employeeId = null, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .Include(p => p.PayrollItems)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(p => p.EmployeeId == employeeId.Value);

            if (departmentId.HasValue)
                query = query.Where(p => p.Employee.DepartmentId == departmentId.Value);

            if (startDate.HasValue)
                query = query.Where(p => p.PayPeriodStart >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PayPeriodEnd <= endDate.Value);

            var payrolls = await query.ToListAsync();

            return payrolls.Select(p => new EmployeeEarningsDto
            {
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                DepartmentName = p.Employee.Department?.Name ?? "N/A",
                Position = p.Employee.Position ?? "N/A",
                BaseSalary = 0, // This would need to be calculated from compensation
                Bonus = 0, // This would need to be calculated from compensation
                Commission = 0, // This would need to be calculated from compensation
                Benefits = 0, // This would need to be calculated from compensation
                TotalEarnings = p.GrossPay,
                GrossPay = p.GrossPay,
                NetPay = p.NetPay,
                TotalDeductions = p.TotalDeductions,
                TotalTaxes = p.TotalTaxes,
                PayPeriodStart = p.PayPeriodStart,
                PayPeriodEnd = p.PayPeriodEnd,
                PayDate = p.PayDate,
                PayrollItems = p.PayrollItems.Select(pi => new PayrollItemDto
                {
                    ItemType = pi.ItemType,
                    Name = pi.Name,
                    Description = pi.Description,
                    Amount = pi.Amount,
                    RateType = pi.RateType,
                    Rate = pi.Rate,
                    Quantity = pi.Quantity
                }).ToList()
            });
        }

        public async Task<IEnumerable<DepartmentPayrollReportDto>> GetDepartmentPayrollReportAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Departments
                .Include(d => d.Employees)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(d => d.Id == departmentId.Value);

            var departments = await query.ToListAsync();
            var result = new List<DepartmentPayrollReportDto>();

            foreach (var dept in departments)
            {
                var employeeIds = dept.Employees.Select(e => e.Id).ToList();
                
                var payrollQuery = _context.Payrolls
                    .Where(p => employeeIds.Contains(p.EmployeeId));

                if (startDate.HasValue)
                    payrollQuery = payrollQuery.Where(p => p.PayPeriodStart >= startDate.Value);

                if (endDate.HasValue)
                    payrollQuery = payrollQuery.Where(p => p.PayPeriodEnd <= endDate.Value);

                var payrolls = await payrollQuery.ToListAsync();

                var totalGrossPay = payrolls.Sum(p => p.GrossPay);
                var totalNetPay = payrolls.Sum(p => p.NetPay);
                var totalDeductions = payrolls.Sum(p => p.TotalDeductions);
                var totalTaxes = payrolls.Sum(p => p.TotalTaxes);
                var overtimeCost = payrolls.Sum(p => p.OvertimeHours * 25); // Assuming $25 per hour

                result.Add(new DepartmentPayrollReportDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    EmployeeCount = dept.Employees.Count,
                    TotalGrossPay = totalGrossPay,
                    TotalNetPay = totalNetPay,
                    TotalDeductions = totalDeductions,
                    TotalTaxes = totalTaxes,
                    AverageGrossPay = dept.Employees.Count > 0 ? totalGrossPay / dept.Employees.Count : 0,
                    AverageNetPay = dept.Employees.Count > 0 ? totalNetPay / dept.Employees.Count : 0,
                    OvertimeCost = overtimeCost,
                    BenefitsCost = 0, // This would need to be calculated from compensation
                    EmployeeSummaries = payrolls.Select(p => new EmployeePayrollSummaryDto
                    {
                        EmployeeId = p.EmployeeId,
                        EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                        Position = p.Employee.Position ?? "N/A",
                        GrossPay = p.GrossPay,
                        NetPay = p.NetPay,
                        Deductions = p.TotalDeductions,
                        Taxes = p.TotalTaxes,
                        OvertimePay = p.OvertimeHours * 25,
                        Benefits = 0 // This would need to be calculated from compensation
                    }).ToList(),
                    ReportDate = DateTime.UtcNow
                });
            }

            return result;
        }

        public async Task<TaxReportDto> GetTaxReportAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(p => p.Employee.DepartmentId == departmentId.Value);

            if (startDate.HasValue)
                query = query.Where(p => p.PayPeriodStart >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PayPeriodEnd <= endDate.Value);

            var payrolls = await query.ToListAsync();

            var totalFederalIncomeTax = payrolls.Sum(p => p.FederalIncomeTax);
            var totalStateIncomeTax = payrolls.Sum(p => p.StateIncomeTax);
            var totalLocalIncomeTax = payrolls.Sum(p => p.LocalIncomeTax);
            var totalSocialSecurityTax = payrolls.Sum(p => p.SocialSecurityTax);
            var totalMedicareTax = payrolls.Sum(p => p.MedicareTax);
            var totalTaxes = totalFederalIncomeTax + totalStateIncomeTax + totalLocalIncomeTax + totalSocialSecurityTax + totalMedicareTax;

            return new TaxReportDto
            {
                TotalFederalIncomeTax = totalFederalIncomeTax,
                TotalStateIncomeTax = totalStateIncomeTax,
                TotalLocalIncomeTax = totalLocalIncomeTax,
                TotalSocialSecurityTax = totalSocialSecurityTax,
                TotalMedicareTax = totalMedicareTax,
                TotalUnemploymentTax = 0, // This would need to be calculated
                TotalTaxes = totalTaxes,
                ByEmployee = payrolls.Select(p => new TaxByEmployeeDto
                {
                    EmployeeId = p.EmployeeId,
                    EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                    DepartmentName = p.Employee.Department?.Name ?? "N/A",
                    GrossPay = p.GrossPay,
                    FederalIncomeTax = p.FederalIncomeTax,
                    StateIncomeTax = p.StateIncomeTax,
                    LocalIncomeTax = p.LocalIncomeTax,
                    SocialSecurityTax = p.SocialSecurityTax,
                    MedicareTax = p.MedicareTax,
                    TotalTaxes = p.FederalIncomeTax + p.StateIncomeTax + p.LocalIncomeTax + p.SocialSecurityTax + p.MedicareTax
                }).ToList(),
                ByDepartment = payrolls.GroupBy(p => p.Employee.Department.Name)
                    .Select(g => new DepartmentTaxSummaryDto
                    {
                        DepartmentName = g.Key ?? "N/A",
                        TotalWages = g.Sum(p => p.GrossPay),
                        TotalFederalTaxWithheld = g.Sum(p => p.FederalIncomeTax),
                        TotalStateTaxWithheld = g.Sum(p => p.StateIncomeTax),
                        TotalLocalTaxWithheld = g.Sum(p => p.LocalIncomeTax)
                    }).ToList(),
                ByMonth = payrolls.GroupBy(p => new { p.PayPeriodStart.Year, p.PayPeriodStart.Month })
                    .Select(g => new MonthlyTaxSummaryDto
                    {
                        Month = g.Key.Month,
                        TotalWages = g.Sum(p => p.GrossPay),
                        TotalFederalTaxWithheld = g.Sum(p => p.FederalIncomeTax),
                        TotalStateTaxWithheld = g.Sum(p => p.StateIncomeTax),
                        TotalLocalTaxWithheld = g.Sum(p => p.LocalIncomeTax)
                    }).ToList(),
                ReportDate = DateTime.UtcNow
            };
        }

        public async Task<byte[]> GeneratePayrollReportAsync(string reportType, int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            // This would generate a PDF or Excel report
            // For now, return empty byte array
            await Task.CompletedTask;
            return new byte[0];
        }
    }
}
