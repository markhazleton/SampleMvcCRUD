using Microsoft.EntityFrameworkCore;

namespace Mwh.Sample.BlazorSPA.Data
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
           : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
    }
}
