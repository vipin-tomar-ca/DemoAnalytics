using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/impact")]
// [Authorize] // Commented out to disable authentication
public class ImpactController : ControllerBase
{
    private readonly PayrollContext _db;

    public ImpactController(PayrollContext db) => _db = db;

    [HttpGet("finance")]
    public async Task<IActionResult> GetFinanceImpact()
    {
        var orgs = await _db.OrgUnits.Select(o => o.Name).ToListAsync();
        var rng = new Random(41);

        var payload = orgs.Select(org => new
        {
            org,
            prRatio = Math.Round(30 + rng.NextDouble() * 25, 1),
            salesPerEmp = rng.Next(120_000, 380_000),
            productivityIndex = rng.Next(1, 10)
        });

        return Ok(new { orgs = payload });
    }
}
