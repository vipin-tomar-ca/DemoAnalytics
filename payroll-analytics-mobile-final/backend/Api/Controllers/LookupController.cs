using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollAnalytics.Api.Controllers
{
    // [Authorize] // Commented out to disable authentication
    [ApiController]
    [Route("api/lookup")]
    public class LookupController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<LookupController> _logger;

        public LookupController(PayrollContext context, ILogger<LookupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            try
            {
                return await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(500, "Internal server error while retrieving departments");
            }
        }

        [HttpGet("job-titles")]
        public async Task<ActionResult<IEnumerable<string>>> GetJobTitles()
        {
            try
            {
                return await _context.Employees
                    .Select(e => e.Position)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job titles");
                return StatusCode(500, "Internal server error while retrieving job titles");
            }
        }

        [HttpGet("absence-types")]
        public async Task<ActionResult<IEnumerable<AbsenceType>>> GetAbsenceTypes()
        {
            try
            {
                return await _context.AbsenceTypes
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving absence types");
                return StatusCode(500, "Internal server error while retrieving absence types");
            }
        }

        [HttpGet("pay-grades")]
        public async Task<ActionResult<IEnumerable<PayGrade>>> GetPayGrades()
        {
            try
            {
                return await _context.PayGrades
                    .OrderBy(g => g.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pay grades");
                return StatusCode(500, "Internal server error while retrieving pay grades");
            }
        }

        [HttpGet("employment-types")]
        public async Task<ActionResult<IEnumerable<string>>> GetEmploymentTypes()
        {
            try
            {
                return await _context.Employees
                    .Select(e => e.EmploymentType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employment types");
                return StatusCode(500, "Internal server error while retrieving employment types");
            }
        }

        [HttpGet("locations")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            try
            {
                return await _context.Locations
                    .OrderBy(l => l.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving locations");
                return StatusCode(500, "Internal server error while retrieving locations");
            }
        }
    }
}
