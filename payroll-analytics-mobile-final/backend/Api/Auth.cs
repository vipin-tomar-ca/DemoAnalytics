using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api;

public static class Auth
{
    public static string Issuer = "PayrollAnalytics";
    public static string Audience = "PayrollAnalyticsUsers";
    public static string Key = "super-secret-demo-key-please-change"; // demo only!

    public static string CreateToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("tenant", user.TenantId)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );
        return handler.WriteToken(token);
    }
}
