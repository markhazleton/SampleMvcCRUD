

namespace Mwh.Sample.Repository.Tests.Models;
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
