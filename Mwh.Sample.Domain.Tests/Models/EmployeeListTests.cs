namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class EmployeeListTests
{
    [TestMethod]
    public void AddItem_Null_ExpectedBehavior()
    {
        // Arrange
        var employeeList = new EmployeeList();
        EmployeeDto? item = null;

        // Act
        var result = employeeList.AddItem(item);

        // Assert
        Assert.AreEqual(result.EnumerateItems().Count(), 0);
    }

    [TestMethod]
    public void AddItem_Valid_ExpectedBehavior()
    {
        // Arrange
        var employeeList = new EmployeeList();
        EmployeeDto? item = new(
            3,
            "Test",
            33,
            "TX",
            "USA",
            EmployeeDepartmentEnum.HR
            );

        // Act
        var result = employeeList.AddItem(item);

        // Assert
        Assert.AreEqual(result.EnumerateItems().Count(), 1);
    }

    [TestMethod]
    public void EnumerateItems_Initialization_ExpectedBehavior()
    {
        // Arrange
        var employeeList = new EmployeeList();

        // Act
        var result = employeeList.EnumerateItems();

        // Assert
        Assert.AreEqual(result.Count(), 0);
    }
}
