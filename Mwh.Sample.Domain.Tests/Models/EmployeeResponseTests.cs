
namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class EmployeeResponseTests
{
    [TestMethod]
    public void EmployeeResponse_Expected()
    {
        // Arrange
        var employee = new EmployeeDto()
        {
            Id = 1,
            Age = 20,
            Name = "Test Employee",
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT
        };
        var employeeResponse = new EmployeeResponse(employee);

        // Act

        // Assert
        Assert.IsNotNull(employeeResponse);
        Assert.AreEqual(employeeResponse.Success, true);
    }
    [TestMethod]
    public void EmployeeResponse_EmptyConstructor()
    {
        // Arrange
        var employeeResponse = new EmployeeResponse();

        // Act

        // Assert
        Assert.IsNotNull(employeeResponse);
        Assert.AreEqual("Empty Initialize", employeeResponse.Message);
        Assert.AreEqual(employeeResponse.Success, false);
    }
}
