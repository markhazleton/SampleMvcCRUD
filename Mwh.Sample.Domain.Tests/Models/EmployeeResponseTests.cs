
namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class EmployeeResponseTests
{
    [TestMethod]
    public void EmployeeResponse_Expected()
    {
        // Arrange
        var employee = new EmployeeDto(
            1,
            "Test Employee",
            20,
            "TX",
            "USA",
            EmployeeDepartmentEnum.IT);
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
