using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Data;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Data;

public static class Seed
{
    public static async Task EnsureDbAsync(PayrollContext db)
    {
        await db.Database.MigrateAsync();
        // if (await db.Users.AnyAsync()) return; // Removed this line

        var hasher = new Func<string, string>(BCrypt.Net.BCrypt.HashPassword);
        db.Users.Add(new User { Username = "admin", PasswordHash = hasher("admin123"), Role = "Admin" });
        db.Users.Add(new User { Username = "analyst", PasswordHash = hasher("analyst123"), Role = "User" });

        // Seed OrgUnits
        string[] orgUnitNames = ["Sales","Engineering","Ops","Support","Finance","HR","Marketing"];
        foreach (var name in orgUnitNames) db.OrgUnits.Add(new OrgUnit { Name = name });
        await db.SaveChangesAsync();
        var allOrgUnits = await db.OrgUnits.ToListAsync();

        // Seed Departments
        string[] departmentNames = ["Sales","Engineering","Human Resources","Finance","Operations","Customer Success","Marketing","Research and Development"];
        foreach (var name in departmentNames) db.Departments.Add(new Department { Name = name });
        await db.SaveChangesAsync();
        var allDepartments = await db.Departments.ToListAsync();

        // Seed Locations
        string[] locationNames = ["Headquarters","Satellite Office A","Remote Hub B","Regional Center C"];
        foreach (var name in locationNames) db.Locations.Add(new Location { Name = name, Address = "123 Main St", City = "Anytown", State = "CA", PostalCode = "90210", Country = "USA", Phone = "555-1234", Email = "info@example.com", IsActive = true });
        await db.SaveChangesAsync();
        var allLocations = await db.Locations.ToListAsync();

        // Seed AbsenceTypes
        string[] absenceTypeNames = ["Sick Leave","Vacation","Personal Leave","Family Leave","Bereavement Leave","Jury Duty","Maternity Leave","Paternity Leave","Sabbatical", "Overtime", "PTO"]; // Added Overtime and PTO
        foreach (var name in absenceTypeNames) db.AbsenceTypes.Add(new AbsenceType { Name = name, Description = name + " leave", IsPaid = true, RequiresApproval = true });
        await db.SaveChangesAsync();
        var allAbsenceTypes = await db.AbsenceTypes.ToListAsync();

        // Seed PayGrades
        string[] payGradeNames = ["Entry Level","Junior","Intermediate","Senior","Lead","Principal"];
        foreach (var name in payGradeNames) db.PayGrades.Add(new PayGrade { Name = name, Description = name + " pay grade", MinSalary = 30000m, MaxSalary = 200000m });
        await db.SaveChangesAsync();
        var allPayGrades = await db.PayGrades.ToListAsync();

        var rnd = new Random(100);
        string[] provinces = ["Ontario","Quebec","British Columbia","Alberta","Manitoba","Saskatchewan","Nova Scotia","New Brunswick","Newfoundland and Labrador","Prince Edward Island","Yukon","Northwest Territories","Nunavut"];
        string[] jobFamilies = ["Engineering","Sales","HR","Finance","Operations","Customer Success"];

        DateTime baseDate = DateTime.UtcNow.AddYears(-2);
        // Employees - First Pass
        List<Employee> employeesToSeed = new List<Employee>();
        for (int i=0;i<500;i++)
        {
            var org = allOrgUnits.OrderBy(_=>Guid.NewGuid()).First();
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
                Position = jobFamilies[rnd.Next(jobFamilies.Length)] + " " + rnd.Next(1, 5),
                EmploymentType = rnd.NextDouble() < 0.8 ? "Full-time" : "Part-time",
                IsActive = term == null,
                HireDate = hire,
                TerminationDate = term,
                DepartmentId = allDepartments.OrderBy(_=>Guid.NewGuid()).First().Id, // Assign DepartmentId
                LocationId = allLocations.OrderBy(_=>Guid.NewGuid()).First().Id // Assign LocationId
            };
            employeesToSeed.Add(e);
        }
        db.Employees.AddRange(employeesToSeed);
        await db.SaveChangesAsync(); // Save all employees to generate Ids

        // Retrieve all employees with their generated IDs
        var allEmployees = await db.Employees.ToListAsync();

        // Now, iterate over the employees to add compensations, absences, employee starts, and exits - Second Pass
        foreach (var e in allEmployees)
        {
            var baseSalary = (decimal)rnd.Next(50000, 150000);
            var compDate = e.HireDate.Value.AddDays(rnd.Next(0, 365));

            // Add to EmployeeStarts
            db.EmployeeStarts.Add(new EmployeeStart { EmployeeId = e.Id, StartDate = e.HireDate.Value, Position = e.Position, Department = e.Department?.Name ?? "", Salary = baseSalary, Reason = "New Hire" });

            // Add to EmployeeExits if terminated
            if (e.TerminationDate.HasValue)
            {
                string[] exitReasons = ["Resignation","Retirement","Better Opportunity","Relocation","Career Change","Dissatisfaction"];
                string[] exitTypes = ["Voluntary","Involuntary"];
                db.EmployeeExits.Add(new EmployeeExit {
                    EmployeeId = e.Id,
                    ExitDate = e.TerminationDate.Value,
                    Reason = exitReasons[rnd.Next(exitReasons.Length)],
                    Type = exitTypes[rnd.Next(exitTypes.Length)]
                });
            }

            // Compensation history
            for (int j=0;j<3;j++)
            {
                db.Compensations.Add(new Compensation {
                    EmployeeId = e.Id,
                    EffectiveDate = compDate,
                    BaseSalary = baseSalary,
                    Bonus = baseSalary * (decimal)(0.05 + rnd.NextDouble()*0.1),
                    Benefits = baseSalary * 0.2m,
                    PayrollTaxes = baseSalary * 0.08m,
                    PayGradeId = allPayGrades.OrderBy(_=>Guid.NewGuid()).First().Id,
                    TotalCompensation = baseSalary + (baseSalary * (decimal)(0.05 + rnd.NextDouble()*0.1)) + (baseSalary * 0.2m) + (baseSalary * 0.08m)
                });
                baseSalary = baseSalary * (decimal)(1 + rnd.NextDouble()*0.05);
                compDate = compDate.AddMonths(8 + rnd.Next(0,6));
            }

            // Absences (including some overtime records)
            int absCount = rnd.Next(2, 20);
            for (int a=0;a<absCount;a++)
            {
                var dt = e.HireDate.Value.AddDays(rnd.Next(0, 600));
                bool ot = rnd.NextDouble() < 0.3;
                var typeString = ot ? "Overtime" : (rnd.NextDouble()<0.5?"Sick":"PTO");
                var hours = ot ? rnd.Next(1, 6) : rnd.Next(4, 9);

                var absenceType = allAbsenceTypes.FirstOrDefault(at => at.Name == typeString) ?? allAbsenceTypes.First();

                db.Absences.Add(new Absence {
                    EmployeeId = e.Id,
                    Date = dt,
                    Type = typeString,
                    Hours = hours,
                    Cost = (decimal)(hours * (baseSalary/2080m) * (ot?1.5m:1.0m)),
                    IsOvertime = ot,
                    AbsenceTypeId = absenceType.Id
                });
            }
        }
        await db.SaveChangesAsync(); // Save all compensations and absences
    }
}
