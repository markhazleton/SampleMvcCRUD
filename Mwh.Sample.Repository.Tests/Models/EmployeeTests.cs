namespace Mwh.Sample.Repository.Tests.Models;

[TestClass]
public class EmployeeTests
{
    [TestMethod]
    public void Employee_Test()
    {
        // Arrange
        var employee = new Employee
        {
            Age = 20,
            Country = "USA",
            DepartmentId = 1,
            Id = 1,
            Name = "Test Employee",
            State = "TX",
            Department = new Department()
            { 
                Id = 1,
                Name="Test",
                Description ="Test"
            }
        };
        // Act
        employee.Age = 21;

        // Assert
        Assert.IsNotNull(employee);
    }
}
