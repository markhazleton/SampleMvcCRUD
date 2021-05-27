using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using Mwh.Sample.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.Core.Data
{
    public class SampleContext : DbContext
    {
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
                optionsBuilder.UseSqlite(@"Data Source=AppData/employeeContext.db");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("Employees");

        }
    }
}
