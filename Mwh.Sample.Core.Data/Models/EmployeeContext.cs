using Microsoft.EntityFrameworkCore;

namespace Mwh.Sample.Core.Data.Models
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext() { }

        public EmployeeContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(@"Data Source=AppData/employeeContext.db");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("Employees");

        }

        public DbSet<Employee> Employees { get; set; }
    }
}
