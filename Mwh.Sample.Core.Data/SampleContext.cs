using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Core.Domain;

namespace Mwh.Sample.Core.Data
{
    public class SampleContext : DbContext
    {
        private string connectionString = "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = MwhSampleTest";

        public SampleContext() { }

        public SampleContext(DbContextOptions options) : base(options)
        { 
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("Employees");
        }
    }
}
