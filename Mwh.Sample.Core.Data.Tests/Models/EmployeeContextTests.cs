
namespace Mwh.Sample.Core.Data.Tests.Models;
[TestClass]
public class EmployeeContextTests
{
    [TestMethod]
    public void EmployeeContext_ExpectedBehavior()
    {
        // Arrange
        var employeeContext = new EmployeeContext();

        // Act

        // Assert
        Assert.IsNotNull(employeeContext);
    }
}
