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
    [Route("api/absences")]
    public class AbsencesController : ControllerBase
    {
        private readonly PayrollContext _context;
        private readonly ILogger<AbsencesController> _logger;

        public AbsencesController(PayrollContext context, ILogger<AbsencesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Absence>>> GetAbsences()
        {
            try
            {
                var absences = await _context.Absences
                    .Include(a => a.Employee)
                    .Include(a => a.AbsenceType)
                    .ToListAsync();
                return Ok(absences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting absences");
                return StatusCode(500, "Internal server error while retrieving absences");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Absence>> GetAbsence(int id)
        {
            try
            {
                var absence = await _context.Absences
                    .Include(a => a.Employee)
                    .Include(a => a.AbsenceType)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (absence == null)
                {
                    return NotFound();
                }

                return Ok(absence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting absence with id {id}");
                return StatusCode(500, "Internal server error while retrieving absence");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Absence>> CreateAbsence(Absence absence)
        {
            try
            {
                _context.Absences.Add(absence);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAbsence), new { id = absence.Id }, absence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating absence");
                return StatusCode(500, "Internal server error while creating absence");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAbsence(int id, Absence absence)
        {
            if (id != absence.Id)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(absence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!AbsenceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency error updating absence with id {id}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating absence with id {id}");
                return StatusCode(500, "Internal server error while updating absence");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAbsence(int id)
        {
            try
            {
                var absence = await _context.Absences.FindAsync(id);
                if (absence == null)
                {
                    return NotFound();
                }

                _context.Absences.Remove(absence);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting absence with id {id}");
                return StatusCode(500, "Internal server error while deleting absence");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Absence>>> GetEmployeeAbsences(int employeeId)
        {
            try
            {
                var absences = await _context.Absences
                    .Where(a => a.EmployeeId == employeeId)
                    .Include(a => a.AbsenceType)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync();

                return Ok(absences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting absences for employee {employeeId}");
                return StatusCode(500, "Internal server error while retrieving employee absences");
            }
        }

        private bool AbsenceExists(int id)
        {
            return _context.Absences.Any(e => e.Id == id);
        }
    }
}
