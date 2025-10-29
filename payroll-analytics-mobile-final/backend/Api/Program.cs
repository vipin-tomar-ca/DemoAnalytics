using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Services;
using PayrollAnalytics.Api.Services.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// Configure DbContext - SQLite (Default)
builder.Services.AddDbContext<PayrollContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection")));

// Register services
builder.Services.AddScoped<IAbsenceService, AbsenceService>();
builder.Services.AddScoped<ICompensationService, CompensationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication - Commented out to disable authentication
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = Auth.Issuer,
//             ValidAudience = Auth.Audience,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Auth.Key))
//         };
//     });
// builder.Services.AddAuthorization(); // Commented out to disable authorization

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapControllers();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PayrollContext>();
    await Seed.EnsureDbAsync(db);
    await DataSeeder.SeedDataAsync(db);
}

app.Run();