namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class EmployeeListTests
{
    [TestMethod]
    public void AddItem_Null_ExpectedBehavior()
    {
        // Arrange
        EmployeeList employeeList = new EmployeeList();
        EmployeeDto? item = null;

        // Act
        EmployeeList result = employeeList.AddItem(item);

        // Assert
        Assert.AreEqual(employeeList.List.Count, 0);
        Assert.AreEqual(result.EnumerateItems().Count(), 0);
    }

    [TestMethod]
    public void AddItem_Valid_ExpectedBehavior()
    {
        // Arrange
        EmployeeList employeeList = new EmployeeList();
        EmployeeDto? item = new(
            3,
            "Test",
            33,
            "TX",
            "USA",
            EmployeeDepartmentEnum.HR
            );

        // Act
        EmployeeList result = employeeList.AddItem(item);

        // Assert
        Assert.AreEqual(employeeList.List.Count, 1);
        Assert.AreEqual(result.EnumerateItems().Count(), 1);
    }

    [TestMethod]
    public void EnumerateItems_Initialization_ExpectedBehavior()
    {
        // Arrange
        EmployeeList employeeList = new EmployeeList();

        // Act
        IEnumerable<EmployeeDto> result = employeeList.EnumerateItems();

        // Assert
        Assert.AreEqual(result.Count(), 0);
        Assert.AreEqual(employeeList.List.Count, 0);
    }
}
