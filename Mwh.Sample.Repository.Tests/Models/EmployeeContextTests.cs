
using Mwh.Sample.Repository.Repository;
using System.Collections.Generic;
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

        employeeMock.DepartmentCollection()?.ForEach(async dept =>
        {
        });

        employeeMock.EmployeeCollection()?.ForEach(async emp =>
        {
        });
        context.Dispose();

        // Assert
        Assert.IsNotNull(context);
    }
}
