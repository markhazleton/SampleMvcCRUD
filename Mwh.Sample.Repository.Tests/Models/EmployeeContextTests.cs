
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
        DbContextOptions<EmployeeContext> options = new DbContextOptionsBuilder<EmployeeContext>()
            .UseInMemoryDatabase("EmployeeTest")
            .Options;
        using EmployeeContext context = new EmployeeContext(options);
        // Act
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        EmployeeMock employeeMock = new EmployeeMock();
        List<EmployeeResponse> employeeList = new List<EmployeeResponse>();
        List<DepartmentResponse> departmentList = new List<DepartmentResponse>();
        EmployeeDatabaseService svc = new EmployeeDatabaseService(context);

        employeeMock.DepartmentCollection()?.ForEach(async dept =>
       {
           departmentList.Add(await svc.SaveDepartmentAsync(dept));
       });
        int deptSuccess = departmentList.Where(w => w.Success).Count();

        employeeMock.EmployeeCollection()?.ForEach(async emp =>
        {
            employeeList.Add(await svc.SaveEmployeeDbAsync(emp));
        });
        int emptSuccess = employeeList.Where(w => w.Success).Count();

        // Assert
        Assert.IsNotNull(context);
        Assert.AreEqual(emptSuccess, await context.Employees.CountAsync());
        Assert.AreEqual(deptSuccess, await context.Departments.CountAsync());

    }
}
