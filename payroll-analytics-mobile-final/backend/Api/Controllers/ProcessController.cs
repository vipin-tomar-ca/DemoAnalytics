using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/process")]
// [Authorize] // Commented out to disable authentication
public class ProcessController : ControllerBase
{
    [HttpGet("efficiency")]
    public IActionResult GetEfficiency()
    {
        // Demo values: would ordinarily originate from payroll run telemetry
        return Ok(new { timeToRunPayrollHours = 12.5, accuracyRatePct = 98.4 });
    }
}
