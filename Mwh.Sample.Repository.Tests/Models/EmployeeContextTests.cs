
using Mwh.Sample.Repository.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Models;
[TestClass]
public class EmployeeContextTests
{
    [TestMethod]
    public async Task EmployeeContext_ExpectedBehaviorAsync()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<EmployeeContext>()
            .UseInMemoryDatabase("EmployeeTest")
            .Options;
        using var context = new EmployeeContext(options);
        // Act
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var employeeMock = new EmployeeMock();
        var employeeList = new List<EmployeeResponse>();
        var departmentList = new List<DepartmentResponse>();
        var svc = new EmployeeDatabaseService(context);

        employeeMock.DepartmentCollection()?.ForEach(async dept =>
       {
           departmentList.Add(await svc.SaveDepartmentAsync(dept));
       });
        var deptSuccess = departmentList.Where(w => w.Success).Count();

        employeeMock.EmployeeCollection()?.ForEach(async emp =>
        {
            employeeList.Add(await svc.SaveEmployeeDbAsync(emp));
        });
        var emptSuccess = employeeList.Where(w => w.Success).Count();

        // Assert
        Assert.IsNotNull(context);
        Assert.AreEqual(emptSuccess, await context.Employees.CountAsync());
        Assert.AreEqual(deptSuccess, await context.Departments.CountAsync());

    }
}
