using System.Security.Claims;

namespace PayrollAnalytics.Api;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "default";
        context.Items["tenant"] = tenantId;
        // In real systems, validate tenant membership from JWT or DB
        await _next(context);
    }
}
