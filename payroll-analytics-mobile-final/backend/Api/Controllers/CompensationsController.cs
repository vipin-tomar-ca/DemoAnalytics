using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollAnalytics.Api.Controllers
{
    // [Authorize] // Commented out to disable authentication
    [ApiController]
    [Route("api/compensations")]
    public class CompensationsController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<CompensationsController> _logger;

        public CompensationsController(PayrollContext context, ILogger<CompensationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compensation>>> GetCompensations()
        {
            try
            {
                var compensations = await _context.Compensations
                    .Include(c => c.Employee)
                    .Include(c => c.PayGrade)
                    .ToListAsync();
                return Ok(compensations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compensations");
                return StatusCode(500, "Internal server error while retrieving compensations");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Compensation>> GetCompensation(int id)
        {
            try
            {
                var compensation = await _context.Compensations
                    .Include(c => c.Employee)
                    .Include(c => c.PayGrade)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compensation == null)
                {
                    return NotFound();
                }

                return Ok(compensation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting compensation with id {id}");
                return StatusCode(500, "Internal server error while retrieving compensation");
            }
        }

        [HttpGet("employee/{employeeId}/current")]
        public async Task<ActionResult<Compensation>> GetCurrentCompensation(int employeeId)
        {
            try
            {
                var compensation = await _context.Compensations
                    .Where(c => c.EmployeeId == employeeId && c.EffectiveDate <= DateTime.UtcNow)
                    .OrderByDescending(c => c.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (compensation == null)
                {
                    return NotFound("No compensation record found for this employee");
                }

                return Ok(compensation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current compensation for employee {employeeId}");
                return StatusCode(500, "Internal server error while retrieving current compensation");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Compensation>> CreateCompensation(Compensation compensation)
        {
            try
            {
                _context.Compensations.Add(compensation);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCompensation), new { id = compensation.Id }, compensation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compensation");
                return StatusCode(500, "Internal server error while creating compensation");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompensation(int id, Compensation compensation)
        {
            if (id != compensation.Id)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(compensation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CompensationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency error updating compensation with id {id}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating compensation with id {id}");
                return StatusCode(500, "Internal server error while updating compensation");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompensation(int id)
        {
            try
            {
                var compensation = await _context.Compensations.FindAsync(id);
                if (compensation == null)
                {
                    return NotFound();
                }

                _context.Compensations.Remove(compensation);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting compensation with id {id}");
                return StatusCode(500, "Internal server error while deleting compensation");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Compensation>>> GetEmployeeCompensations(int employeeId)
        {
            try
            {
                var compensations = await _context.Compensations
                    .Where(c => c.EmployeeId == employeeId)
                    .Include(c => c.PayGrade)
                    .OrderByDescending(c => c.EffectiveDate)
                    .ToListAsync();

                return Ok(compensations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting compensations for employee {employeeId}");
                return StatusCode(500, "Internal server error while retrieving employee compensations");
            }
        }

        private bool CompensationExists(int id)
        {
            return _context.Compensations.Any(e => e.Id == id);
        }
    }
}
