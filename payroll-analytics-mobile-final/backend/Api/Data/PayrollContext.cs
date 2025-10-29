using Microsoft.EntityFrameworkCore;
using PayrollAnalytics.Api.Models;

namespace PayrollAnalytics.Api.Data
{
    public class PayrollContext : DbContext
    {
        public PayrollContext(DbContextOptions<PayrollContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PayrollItem> PayrollItems { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<AbsenceType> AbsenceTypes { get; set; }
        public DbSet<Compensation> Compensations { get; set; }
        public DbSet<PayGrade> PayGrades { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OrgUnit> OrgUnits { get; set; }
        public DbSet<EmployeeStart> EmployeeStarts { get; set; }
        public DbSet<EmployeeExit> EmployeeExits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Absences)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.AbsenceType)
                .WithMany(at => at.Absences)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compensation>()
                .HasOne(c => c.Employee)
                .WithMany(e => e.Compensations)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Compensation>()
                .HasOne(c => c.PayGrade)
                .WithMany(pg => pg.Compensations)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
