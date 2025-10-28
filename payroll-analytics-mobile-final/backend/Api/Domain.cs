using Microsoft.EntityFrameworkCore;

namespace PayrollAnalytics.Api;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeStart> EmployeeStarts => Set<EmployeeStart>();
    public DbSet<EmployeeExit> EmployeeExits => Set<EmployeeExit>();
    public DbSet<EmployeeDataChange> EmployeeDataChanges => Set<EmployeeDataChange>();
    public DbSet<Absence> Absences => Set<Absence>();
    public DbSet<Compensation> Compensations => Set<Compensation>();
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>().HasIndex(u => u.Username).IsUnique();
        mb.Entity<Employee>().HasIndex(e => e.EmployeeNumber).IsUnique();
        mb.Entity<Employee>().HasOne(e => e.OrgUnit).WithMany().HasForeignKey(e => e.OrgUnitId);
        mb.Entity<Employee>().Property(e => e.Status).HasDefaultValue("Active");
        mb.Entity<EmployeeStart>().HasIndex(s => new { s.EmployeeId, s.StartDate });
        mb.Entity<EmployeeExit>().HasIndex(s => new { s.EmployeeId, s.ExitDate });
        mb.Entity<EmployeeDataChange>().HasIndex(c => new { c.EmployeeId, c.ChangeDate });
        mb.Entity<Absence>().HasIndex(a => new { a.EmployeeId, a.Date });
        mb.Entity<Compensation>().HasIndex(c => new { c.EmployeeId, c.EffectiveDate });
    }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public string TenantId { get; set; } = "demo-tenant";
}

public class OrgUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Employee
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Province { get; set; } = "Ontario";
    public int OrgUnitId { get; set; }
    public OrgUnit? OrgUnit { get; set; }
    public string JobFamily { get; set; } = "General";
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Status { get; set; } = "Active"; // Active/Terminated/Leave
}

public class EmployeeStart
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime StartDate { get; set; }
    public string Reason { get; set; } = "Hire";
}

public class EmployeeExit
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime ExitDate { get; set; }
    public string Reason { get; set; } = "Voluntary";
}

public class EmployeeDataChange
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime ChangeDate { get; set; }
    public string Field { get; set; } = "";
    public string OldValue { get; set; } = "";
    public string NewValue { get; set; } = "";
}

public class Absence
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = "Sick"; // or PTO, LOA
    public double Hours { get; set; }
    public double Cost { get; set; }
    public bool IsOvertime { get; set; } = false;
}

public class Compensation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Benefits { get; set; }
    public decimal PayrollTaxes { get; set; }
}
