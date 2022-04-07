
namespace Mwh.Sample.Domain.Tests.Models;

/// <summary>
/// Defines test class EmployeeModelTests.
/// </summary>
[TestClass]
public class EmployeeModelTests
{
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_Validate()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 20,
            State = "State",
            Country = "Country",
            Department = EmployeeDepartmentEnum.Marketing,
            Id = 0,
            Name = "Name"
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsTrue(employeeModel.IsValid);
        Assert.AreEqual(employeeModel.Name, "Name");
        Assert.AreEqual(employeeModel.State, "State");
        Assert.AreEqual(employeeModel.Country, "Country");
        Assert.AreEqual(employeeModel.Department, EmployeeDepartmentEnum.Marketing);
        Assert.AreEqual(employeeModel.Age, 20);
    }
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_IsValidFalse_Name()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 20,
            State = "State",
            Country = "Country",
            Department = EmployeeDepartmentEnum.Marketing,
            Id = 0,
            Name = string.Empty
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsFalse(employeeModel.IsValid);
    }
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_IsValidFalse_State()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 20,
            State = string.Empty,
            Country = "Country",
            Department = EmployeeDepartmentEnum.Marketing,
            Id = 0,
            Name = "Test"
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsFalse(employeeModel.IsValid);
    }
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_IsValidFalse_Country()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 20,
            State = "TX",
            Country = string.Empty,
            Department = EmployeeDepartmentEnum.Marketing,
            Id = 0,
            Name = "Test"
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsFalse(employeeModel.IsValid);
    }
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_IsValidFalse_Age()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 0,
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.Marketing,
            Id = 0,
            Name = "Test"
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsFalse(employeeModel.IsValid);
    }
    /// <summary>
    /// Defines the test method EmployeeModel_Validate.
    /// </summary>
    [TestMethod]
    public void EmployeeModel_IsValidFalse_Department()
    {
        // Arrange
        var employeeModel = new EmployeeDto()
        {
            Age = 20,
            State = "TX",
            Country = "USA",
            Id = 0,
            Name = "Test"
        };

        // Act

        // Assert
        Assert.IsNotNull(employeeModel);
        Assert.IsFalse(employeeModel.IsValid);
    }

}
