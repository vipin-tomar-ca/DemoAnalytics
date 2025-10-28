using Microsoft.OpenApi.Models;
using PayrollAnalytics.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core SQLite
builder.Services.AddDbContext<AppDb>(opt => opt.UseSqlite("Data Source=app.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payroll Analytics API", Version = "v1" }); });

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
    {
        opts.RequireHttpsMetadata = false;
        opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Auth.Issuer,
            ValidAudience = Auth.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Auth.Key))
        };
        opts.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/realtime"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// DB init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    await Seed.EnsureDbAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<RealtimeHub>("/hubs/realtime").RequireAuthorization();

// ---- Auth ----
app.MapPost("/api/auth/login", async (AppDb db, LoginDto dto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
    if (user is null) return Results.Unauthorized();
    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return Results.Unauthorized();
    var token = Auth.CreateToken(user);
    return Results.Ok(new { token, username = user.Username, role = user.Role });
}).AllowAnonymous();

// ---- DB-backed endpoints ----
app.MapGet("/api/kpis", async (AppDb db, DateTime? from, DateTime? to) =>
{
    to ??= DateTime.UtcNow;
    from ??= to.Value.AddMonths(-1);
    // Headcount = employees employed at 'to'
    var headcount = await db.Employees.CountAsync(e => e.HireDate <= to && (e.TerminationDate == null || e.TerminationDate > to));
    var hiresMtd = await db.EmployeeStarts.CountAsync(s => s.StartDate >= new DateTime(to.Value.Year, to.Value.Month, 1) && s.StartDate <= to);
    var exitsMtd = await db.EmployeeExits.CountAsync(s => s.ExitDate >= new DateTime(to.Value.Year, to.Value.Month, 1) && s.ExitDate <= to);
    var overtimeHours = await db.Absences.Where(a => a.IsOvertime && a.Date >= from && a.Date <= to).SumAsync(a => a.Hours);
    var totalHours = await db.Absences.Where(a => a.Date >= from && a.Date <= to).SumAsync(a => a.Hours);
    var overtimePct = totalHours > 0 ? (double)overtimeHours * 100.0 / totalHours : 0.0;
    var avgSalary = await db.Compensations.GroupBy(c => c.EmployeeId).Select(g => g.OrderByDescending(c => c.EffectiveDate).First().BaseSalary).DefaultIfEmpty(0m).AverageAsync(v => (double)v);
    return Results.Ok(new { headcount, hiresMtd, exitsMtd, overtimePct, avgSalary });
}).RequireAuthorization();

app.MapGet("/api/headcount/trend", async (AppDb db) =>
{
    var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
    var labelStr = labels.Select(d => d.ToString("MMM yy")).ToArray();
    var headcount = new List<int>();
    var hires = new List<int>();
    var exits = new List<int>();

    foreach (var ts in labels)
    {
        var end = new DateTime(ts.Year, ts.Month, DateTime.DaysInMonth(ts.Year, ts.Month));
        var start = new DateTime(ts.Year, ts.Month, 1);
        var hc = await db.Employees.CountAsync(e => e.HireDate <= end && (e.TerminationDate == null || e.TerminationDate > end));
        var h = await db.EmployeeStarts.CountAsync(s => s.StartDate >= start && s.StartDate <= end);
        var x = await db.EmployeeExits.CountAsync(s => s.ExitDate >= start && s.ExitDate <= end);
        headcount.Add(hc); hires.Add(h); exits.Add(x);
    }
    return Results.Ok(new { labels = labelStr, headcount, hires, exits });
}).RequireAuthorization();

app.MapGet("/api/geo/headcount", async (AppDb db) =>
{
    var q = await db.Employees
        .GroupBy(e => e.Province)
        .Select(g => new { name = g.Key, value = g.Count() })
        .ToListAsync();
    return Results.Ok(q);
}).RequireAuthorization();

app.MapGet("/api/time/heatmap", async (AppDb db) =>
{
    // Build heatmap from overtime hours by day-of-week / hour
    string[] days = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"];
    string[] hours = Enumerable.Range(0,24).Select(h=>$"{h:00}:00").ToArray();
    var values = new List<int[]>();
    foreach (var a in await db.Absences.Where(a=>a.IsOvertime).ToListAsync())
    {
        var d = ((int)a.Date.DayOfWeek + 6) % 7; // make Monday=0
        var h = a.Date.Hour;
        values.Add(new[]{ h, d, (int)Math.Round(a.Hours) });
    }
    return Results.Ok(new { days, hours, values });
}).RequireAuthorization();

app.MapGet("/api/compensation/summary", async (AppDb db) =>
{
    var categories = await db.Employees.Select(e => e.JobFamily).Distinct().ToListAsync();
    var boxData = new List<int[]>();
    foreach (var cat in categories)
    {
        var salaries = await db.Employees.Where(e => e.JobFamily == cat)
            .Join(db.Compensations, e => e.Id, c => c.EmployeeId, (e, c) => c)
            .GroupBy(c => c.EmployeeId)
            .Select(g => g.OrderByDescending(c => c.EffectiveDate).First().BaseSalary)
            .Select(s => (int)s)
            .OrderBy(s => s).ToListAsync();
        if (salaries.Count == 0) { boxData.Add(new[]{0,0,0,0,0}); continue; }
        int min = salaries.First();
        int max = salaries.Last();
        int q1 = salaries[(int)(0.25*(salaries.Count-1))];
        int median = salaries[(int)(0.5*(salaries.Count-1))];
        int q3 = salaries[(int)(0.75*(salaries.Count-1))];
        boxData.Add(new[]{min,q1,median,q3,max});
    }
    return Results.Ok(new { categories, boxData });
}).RequireAuthorization();

// Visier-like KPI routes using DB where relevant
app.MapGet("/api/costs/tcow", async (AppDb db) =>
{
    var latestComp = await db.Compensations.GroupBy(c => c.EmployeeId).Select(g => g.OrderByDescending(c => c.EffectiveDate).First()).ToListAsync();
    var totalBase = latestComp.Sum(c => c.BaseSalary);
    var totalBonus = latestComp.Sum(c => c.Bonus);
    var totalBenefits = latestComp.Sum(c => c.Benefits);
    var totalTaxes = latestComp.Sum(c => c.PayrollTaxes);
    var training = (decimal)(latestComp.Count * 800);
    var travel = (decimal)(latestComp.Count * 500);

    return Results.Ok(new {
        total = totalBase + totalBonus + totalBenefits + totalTaxes + training + travel,
        breakdown = new object[] {
            new { name = "Base Pay", value = totalBase },
            new { name = "Bonuses", value = totalBonus },
            new { name = "Benefits", value = totalBenefits },
            new { name = "Payroll Taxes", value = totalTaxes },
            new { name = "Training", value = training },
            new { name = "Travel", value = travel }
        }
    });
}).RequireAuthorization();

app.MapGet("/api/costs/budget-variance", async (AppDb db) =>
{
    var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
    var labelStr = labels.Select(d => d.ToString("MMM")).ToArray();
    var rnd = new Random(22);
    var budget = labelStr.Select(_ => rnd.Next(1400000, 1600000)).ToArray();
    // Actual based on DB TCOW pieces + noise
    var latestComp = await db.Compensations.GroupBy(c => c.EmployeeId).Select(g => g.OrderByDescending(c => c.EffectiveDate).First()).ToListAsync();
    var baseTotal = (int)latestComp.Sum(c => c.BaseSalary + c.Bonus + c.Benefits + c.PayrollTaxes);
    var actual = budget.Select(b => Math.Max(0, (int)(b * (0.9 + rnd.NextDouble()*0.2)))).ToArray();
    var variancePct = actual.Zip(budget, (a,b) => Math.Round((a - b) * 100.0 / b, 1)).ToArray();
    return Results.Ok(new { labels = labelStr, budget, actual, variancePct });
}).RequireAuthorization();

app.MapGet("/api/costs/overtime", async (AppDb db) =>
{
    var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
    var labelStr = labels.Select(d => d.ToString("MMM")).ToArray();
    var costs = new List<int>();
    var hours = new List<int>();
    foreach (var month in labels)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        var abs = await db.Absences.Where(a => a.IsOvertime && a.Date >= start && a.Date <= end).ToListAsync();
        costs.Add((int)abs.Sum(a => a.Cost));
        hours.Add((int)abs.Sum(a => a.Hours));
    }
    return Results.Ok(new { labels = labelStr, costs, hours });
}).RequireAuthorization();

app.MapGet("/api/costs/absenteeism", async (AppDb db) =>
{
    var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
    var labelStr = labels.Select(d => d.ToString("MMM")).ToArray();
    var costs = new List<int>();
    var ratePct = new List<double>();
    foreach (var month in labels)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        var abs = await db.Absences.Where(a => !a.IsOvertime && a.Date >= start && a.Date <= end).ToListAsync();
        costs.Add((int)abs.Sum(a => a.Cost));
        // naive rate = hours absent / (employees * 160)
        var empCount = await db.Employees.CountAsync(e => e.HireDate <= end && (e.TerminationDate == null || e.TerminationDate > end));
        var totalHours = empCount * 160.0;
        var rate = totalHours > 0 ? Math.Round(abs.Sum(a => a.Hours) * 100.0 / totalHours, 2) : 0.0;
        ratePct.Add(rate);
    }
    return Results.Ok(new { labels = labelStr, costs, ratePct });
}).RequireAuthorization();

app.MapGet("/api/process/efficiency", async (AppDb db) =>
{
    // demo fixed; could derive from payroll_run_logs table
    return Results.Ok(new { timeToRunPayrollHours = 12.5, accuracyRatePct = 98.4 });
}).RequireAuthorization();

app.MapGet("/api/comp/competitiveness", async (AppDb db) =>
{
    // Create simulated compa-ratio points from salary vs assumed midpoints by job family
    var midpoints = new Dictionary<string, decimal> {
        ["Engineering"] = 110000m, ["Sales"] = 95000m, ["HR"] = 80000m, ["Finance"] = 95000m, ["Operations"] = 85000m, ["Customer Success"] = 75000m
    };
    var q = await db.Employees.Select(e => new {
        e.HireDate, e.JobFamily, Latest = db.Compensations.Where(c => c.EmployeeId == e.Id).OrderByDescending(c => c.EffectiveDate).FirstOrDefault()
    }).ToListAsync();
    var points = q.Where(x => x.Latest != null).Select(x => new [] {
        Math.Round((DateTime.UtcNow - x.HireDate).TotalDays / 365.0, 1),
        Math.Round((double)(x.Latest!.BaseSalary / (midpoints.TryGetValue(x.JobFamily, out var m) ? m : 90000m)), 2),
        new Random(x.HireDate.Day).Next(1,6)
    }).ToArray();
    return Results.Ok(new { points });
}).RequireAuthorization();

app.MapGet("/api/equity/paygap", async (AppDb db) =>
{
    // Simulate a wage-gap bridge
    var steps = new [] {
        new { label = "Raw Gap", delta = -10.0 },
        new { label = "Job Mix", delta = 3.5 },
        new { label = "Tenure", delta = 2.0 },
        new { label = "Performance", delta = 1.2 },
        new { label = "Geo/Market", delta = 1.0 },
        new { label = "Residual Gap", delta = -2.3 }
    };
    return Results.Ok(new { steps });
}).RequireAuthorization();

app.MapGet("/api/impact/finance", async (AppDb db) =>
{
    // Scatter based on orgs
    var orgs = await db.OrgUnits.Select(o => o.Name).ToListAsync();
    var rnd = new Random(41);
    var data = orgs.Select(o => new {
        org = o,
        prRatio = Math.Round(30 + rnd.NextDouble()*25, 1),
        salesPerEmp = rnd.Next(120000, 380000),
        productivityIndex = rnd.Next(1,10)
    });
    return Results.Ok(new { orgs = data });
}).RequireAuthorization();

app.MapGet("/api/turnover/costs", async (AppDb db) =>
{
    var labels = Enumerable.Range(0, 12).Select(i => DateTime.UtcNow.AddMonths(-11 + i)).ToArray();
    var labelStr = labels.Select(d => d.ToString("MMM")).ToArray();
    var replacementCost = new List<int>();
    var voluntaryPct = new List<double>();
    var involuntaryPct = new List<double>();

    foreach (var month in labels)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        var exits = await db.EmployeeExits.Where(e => e.ExitDate >= start && e.ExitDate <= end).ToListAsync();
        replacementCost.Add(exits.Count * 25000); // naive per‑head replacement cost
        // derive voluntary/involuntary rates
        var vol = exits.Count(e => e.Reason == "Voluntary");
        var invol = exits.Count - vol;
        var empCount = await db.Employees.CountAsync(e => e.HireDate <= end && (e.TerminationDate == null || e.TerminationDate > end));
        voluntaryPct.Add(empCount>0 ? Math.Round(vol * 100.0 / empCount, 2) : 0);
        involuntaryPct.Add(empCount>0 ? Math.Round(invol * 100.0 / empCount, 2) : 0);
    }
    return Results.Ok(new { labels = labelStr, replacementCost, voluntaryPct, involuntaryPct });
}).RequireAuthorization();

app.Run("http://localhost:5188");

record LoginDto(string Username, string Password);
