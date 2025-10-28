using Microsoft.EntityFrameworkCore;

namespace PayrollAnalytics.Api;

public static class Seed
{
    public static async Task EnsureDbAsync(AppDb db)
    {
        await db.Database.MigrateAsync();
        if (await db.Users.AnyAsync()) return;

        var hasher = BCrypt.Net.BCrypt.HashPassword;
        db.Users.Add(new User { Username = "admin", PasswordHash = hasher("admin123"), Role = "Admin" });
        db.Users.Add(new User { Username = "analyst", PasswordHash = hasher("analyst123"), Role = "User" });

        var orgs = new [] { "Sales","Engineering","Ops","Support","Finance","HR","Marketing" };
        foreach (var o in orgs) db.OrgUnits.Add(new OrgUnit { Name = o });
        await db.SaveChangesAsync();

        var rnd = new Random(100);
        string[] provinces = ["Ontario","Quebec","British Columbia","Alberta","Manitoba","Saskatchewan","Nova Scotia","New Brunswick","Newfoundland and Labrador","Prince Edward Island","Yukon","Northwest Territories","Nunavut"];
        string[] jobFamilies = ["Engineering","Sales","HR","Finance","Operations","Customer Success"];

        DateTime baseDate = DateTime.UtcNow.AddYears(-2);
        // Employees
        for (int i=0;i<500;i++)
        {
            var org = await db.OrgUnits.OrderBy(_=>Guid.NewGuid()).FirstAsync();
            var hire = baseDate.AddDays(rnd.Next(0, 700));
            DateTime? term = null;
            if (rnd.NextDouble() < 0.15) term = hire.AddDays(rnd.Next(60, 600));
            var e = new Employee {
                EmployeeNumber = $"E{i:00000}",
                FirstName = "Emp",
                LastName = $"{i:000}",
                Province = provinces[rnd.Next(provinces.Length)],
                OrgUnitId = org.Id,
                JobFamily = jobFamilies[rnd.Next(jobFamilies.Length)],
                HireDate = hire,
                TerminationDate = term,
                Status = term.HasValue ? "Terminated" : "Active"
            };
            db.Employees.Add(e);
            await db.SaveChangesAsync();
            db.EmployeeStarts.Add(new EmployeeStart { EmployeeId = e.Id, StartDate = hire, Reason = "Hire" });
            if (term.HasValue) db.EmployeeExits.Add(new EmployeeExit { EmployeeId = e.Id, ExitDate = term.Value, Reason = rnd.NextDouble()<0.7 ? "Voluntary" : "Involuntary" });

            // Compensation history
            var compDate = hire;
            decimal baseSalary = rnd.Next(50_000, 140_000);
            for (int j=0;j<3;j++)
            {
                db.Compensations.Add(new Compensation {
                    EmployeeId = e.Id,
                    EffectiveDate = compDate,
                    BaseSalary = baseSalary,
                    Bonus = (decimal)(baseSalary * (0.05 + rnd.NextDouble()*0.1)),
                    Benefits = (decimal)(baseSalary * 0.2),
                    PayrollTaxes = (decimal)(baseSalary * 0.08)
                });
                baseSalary = (decimal)(baseSalary * (1 + rnd.NextDouble()*0.05));
                compDate = compDate.AddMonths(8 + rnd.Next(0,6));
            }

            // Absences (including some overtime records)
            int absCount = rnd.Next(2, 20);
            for (int a=0;a<absCount;a++)
            {
                var dt = hire.AddDays(rnd.Next(0, 600));
                bool ot = rnd.NextDouble() < 0.3;
                var hours = ot ? rnd.Next(1, 6) : rnd.Next(4, 9);
                db.Absences.Add(new Absence {
                    EmployeeId = e.Id,
                    Date = dt,
                    Type = ot ? "Overtime" : (rnd.NextDouble()<0.5?"Sick":"PTO"),
                    Hours = hours,
                    Cost = (double)(hours * (double)(baseSalary/2080m) * (ot?1.5:1.0)),
                    IsOvertime = ot
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
