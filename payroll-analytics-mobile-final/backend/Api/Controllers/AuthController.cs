using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Controllers;

[ApiController]
[Route("api/auth")]
// [Authorize] // Commenting out authorization for now
public class AuthController : ControllerBase
{
    private readonly PayrollContext _db;

    public AuthController(PayrollContext db) => _db = db;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null) return Unauthorized();
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return Unauthorized();

        var token = Auth.CreateToken(user);
        return Ok(new { token, username = user.Username, role = user.Role });
    }
}
