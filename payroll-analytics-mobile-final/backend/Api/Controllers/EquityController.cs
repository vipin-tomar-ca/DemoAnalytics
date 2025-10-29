using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/equity")]
// [Authorize] // Commented out to disable authentication
public class EquityController : ControllerBase
{
    [HttpGet("paygap")]
    public IActionResult GetPayGapBridge()
    {
        var steps = new[]
        {
            new { label = "Raw Gap", delta = -10.0 },
            new { label = "Job Mix", delta = 3.5 },
            new { label = "Tenure", delta = 2.0 },
            new { label = "Performance", delta = 1.2 },
            new { label = "Geo/Market", delta = 1.0 },
            new { label = "Residual Gap", delta = -2.3 }
        };

        return Ok(new { steps });
    }
}
