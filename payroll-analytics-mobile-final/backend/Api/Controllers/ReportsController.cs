using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using PayrollAnalytics.Api.DTOs;

namespace PayrollAnalytics.Api.Controllers
{
    // [Authorize] // Commented out to disable authentication
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(PayrollContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [HttpGet("payroll-summary")]
        public async Task<IActionResult> GeneratePayrollSummaryReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var payrollData = await _context.Payrolls
                    .Where(p => p.PayPeriodStart >= startDate && p.PayPeriodEnd <= endDate)
                    .Include(p => p.Employee)
                    .Include(p => p.PayrollItems)
                    .ToListAsync();

                var reportData = payrollData.Select(p => new PayrollSummaryReportDto
                {
                    EmployeeId = p.EmployeeId,
                    EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                    Department = p.Employee.Department?.Name ?? "N/A",
                    PayPeriod = $"{p.PayPeriodStart:MM/dd/yyyy} - {p.PayPeriodEnd:MM/dd/yyyy}",
                    GrossPay = p.GrossPay,
                    NetPay = p.NetPay,
                    TotalDeductions = p.TotalDeductions,
                    TotalTaxes = p.TotalTaxes,
                    PayDate = p.PayDate
                }).ToList();

                return Ok(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payroll summary report");
                return StatusCode(500, "Internal server error while generating payroll summary report");
            }
        }

        [HttpGet("payroll-summary/export")]
        public async Task<IActionResult> ExportPayrollSummaryReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var payrollData = await _context.Payrolls
                    .Where(p => p.PayPeriodStart >= startDate && p.PayPeriodEnd <= endDate)
                    .Include(p => p.Employee)
                    .ThenInclude(e => e.Department)
                    .ToListAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Payroll Summary");

                // Add headers
                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 2].Value = "Employee Name";
                worksheet.Cells[1, 3].Value = "Department";
                worksheet.Cells[1, 4].Value = "Pay Period";
                worksheet.Cells[1, 5].Value = "Gross Pay";
                worksheet.Cells[1, 6].Value = "Net Pay";
                worksheet.Cells[1, 7].Value = "Total Deductions";
                worksheet.Cells[1, 8].Value = "Total Taxes";
                worksheet.Cells[1, 9].Value = "Pay Date";

                // Add data
                int row = 2;
                foreach (var item in payrollData)
                {
                    worksheet.Cells[row, 1].Value = item.EmployeeId;
                    worksheet.Cells[row, 2].Value = $"{item.Employee.FirstName} {item.Employee.LastName}";
                    worksheet.Cells[row, 3].Value = item.Employee.Department?.Name ?? "N/A";
                    worksheet.Cells[row, 4].Value = $"{item.PayPeriodStart:MM/dd/yyyy} - {item.PayPeriodEnd:MM/dd/yyyy}";
                    worksheet.Cells[row, 5].Value = item.GrossPay;
                    worksheet.Cells[row, 6].Value = item.NetPay;
                    worksheet.Cells[row, 7].Value = item.TotalDeductions;
                    worksheet.Cells[row, 8].Value = item.TotalTaxes;
                    worksheet.Cells[row, 9].Value = item.PayDate.ToString("MM/dd/yyyy");
                    row++;
                }

                // Format the header
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                worksheet.Cells.AutoFitColumns();

                var fileName = $"PayrollSummary_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting payroll summary report");
                return StatusCode(500, "Internal server error while exporting payroll summary report");
            }
        }

        [HttpGet("employee-earnings/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EmployeeEarningsDto>>> GetEmployeeEarningsReport(
            int employeeId, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddYears(-1);
                endDate ??= DateTime.UtcNow;

                var earnings = await _context.Payrolls
                    .Where(p => p.EmployeeId == employeeId && 
                              p.PayPeriodEnd >= startDate && 
                              p.PayPeriodEnd <= endDate)
                    .OrderBy(p => p.PayPeriodEnd)
                    .Select(p => new EmployeeEarningsDto
                    {
                        PayPeriodEnd = p.PayPeriodEnd,
                        GrossPay = p.GrossPay,
                        NetPay = p.NetPay,
                        RegularHours = (double)p.RegularHours,
                        OvertimeHours = (double)p.OvertimeHours,
                        PayDate = p.PayDate
                    })
                    .ToListAsync();

                return Ok(earnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating employee earnings report for employee {employeeId}");
                return StatusCode(500, "Internal server error while generating employee earnings report");
            }
        }

        [HttpGet("department-payroll/{departmentId}")]
        public async Task<ActionResult<DepartmentPayrollReportDto>> GetDepartmentPayrollReport(
            int departmentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-12);
                endDate ??= DateTime.UtcNow;

                var department = await _context.Departments
                    .Include(d => d.Employees)
                    .ThenInclude(e => e.Payrolls
                        .Where(p => p.PayPeriodEnd >= startDate && p.PayPeriodEnd <= endDate))
                    .FirstOrDefaultAsync(d => d.Id == departmentId);

                if (department == null)
                {
                    return NotFound("Department not found");
                }

                var report = new DepartmentPayrollReportDto
                {
                    DepartmentId = department.Id,
                    DepartmentName = department.Name,
                    ReportPeriod = $"{startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}",
                    TotalEmployees = department.Employees.Count,
                    TotalPayrollCost = department.Employees
                        .SelectMany(e => e.Payrolls)
                        .Sum(p => p.GrossPay),
                    EmployeePayrolls = department.Employees.Select(e => new EmployeePayrollSummaryDto
                    {
                        EmployeeId = e.Id,
                        EmployeeName = $"{e.FirstName} {e.LastName}",
                        Position = e.Position,
                        TotalEarnings = e.Payrolls.Sum(p => p.GrossPay),
                        AveragePay = e.Payrolls.Any() ? e.Payrolls.Average(p => p.GrossPay) : 0,
                        LastPayDate = e.Payrolls.Any() ? e.Payrolls.Max(p => p.PayDate) : DateTime.MinValue
                    }).OrderByDescending(e => e.TotalEarnings).ToList()
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating department payroll report for department {departmentId}");
                return StatusCode(500, "Internal server error while generating department payroll report");
            }
        }

        [HttpGet("tax-report")]
        public async Task<ActionResult<TaxReportDto>> GetTaxReport(
            [FromQuery] int? year = null,
            [FromQuery] int? quarter = null)
        {
            try
            {
                var reportYear = year ?? DateTime.UtcNow.Year;
                DateTime startDate, endDate;

                if (quarter.HasValue)
                {
                    var quarterStartMonth = (quarter.Value - 1) * 3 + 1;
                    startDate = new DateTime(reportYear, quarterStartMonth, 1);
                    endDate = startDate.AddMonths(3).AddDays(-1);
                }
                else
                {
                    startDate = new DateTime(reportYear, 1, 1);
                    endDate = new DateTime(reportYear, 12, 31);
                }

                var payrolls = await _context.Payrolls
                    .Where(p => p.PayDate >= startDate && p.PayDate <= endDate)
                    .Include(p => p.Employee)
                    .ToListAsync();

                var report = new TaxReportDto
                {
                    Year = reportYear,
                    Quarter = quarter ?? 0,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalWages = payrolls.Sum(p => p.GrossPay),
                    TotalFederalTaxWithheld = payrolls.Sum(p => p.FederalIncomeTax + p.SocialSecurityTax + p.MedicareTax),
                    TotalStateTaxWithheld = payrolls.Sum(p => p.StateIncomeTax),
                    TotalLocalTaxWithheld = payrolls.Sum(p => p.LocalIncomeTax),
                    EmployeeTaxSummaries = payrolls
                        .GroupBy(p => p.EmployeeId)
                        .Select(g => new EmployeeTaxSummaryDto
                        {
                            EmployeeId = g.Key,
                            EmployeeName = $"{g.First().Employee.FirstName} {g.First().Employee.LastName}",
                            SocialSecurityNumber = g.First().Employee.SocialSecurityNumber,
                            TotalWages = g.Sum(p => p.GrossPay),
                            FederalIncomeTax = g.Sum(p => p.FederalIncomeTax),
                            SocialSecurityTax = g.Sum(p => p.SocialSecurityTax),
                            MedicareTax = g.Sum(p => p.MedicareTax),
                            StateIncomeTax = g.Sum(p => p.StateIncomeTax),
                            LocalIncomeTax = g.Sum(p => p.LocalIncomeTax)
                        })
                        .OrderBy(e => e.EmployeeName)
                        .ToList()
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tax report");
                return StatusCode(500, "Internal server error while generating tax report");
            }
        }
    }
}
