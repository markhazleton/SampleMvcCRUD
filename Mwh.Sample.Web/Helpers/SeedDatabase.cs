
namespace Mwh.Sample.Web.Helpers;

/// <summary>
/// See Employee Database
/// </summary>
public static class SeedDatabase
{
    /// <summary>
    /// ConfirmDatabaseCreation
    /// </summary>
    /// <param name="employee"></param>
    public static void ConfirmDatabaseCreation(string databaseName)
    {
        var dbOptions = new DbContextOptionsBuilder<EmployeeContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        var context = new EmployeeContext(dbOptions);
        context.Database.EnsureDeletedAsync();
        context.Database.EnsureCreatedAsync();
        context.Employees.Add(new Employee() { Name = "Ilsa Lund", Age = 25, Country = "Germany", DepartmentId = 1, State = "TX" });
        context.Employees.Add(new Employee() { Name = "Major Strasser", Age = 35, Country = "Germany", DepartmentId = 1, State = "TX" });
        context.Employees.Add(new Employee() { Name = "Rick Blaine", Age = 45, Country = "USA", DepartmentId = 1, State = "TX" });
        context.Employees.Add(new Employee() { Name = "Victor Laszlo", Age = 55, Country = "Germany", DepartmentId = 2, State = "TX" });
        context.Employees.Add(new Employee() { Name = "Louis Renault", Age = 65, Country = "France", DepartmentId = 3, State = "TX" });
        context.Employees.Add(new Employee() { Name = "Sam", Age = 55, Country = "USA", DepartmentId = 1, State = "TX" });
        context.SaveChanges();
    }


}
