
namespace Mwh.Sample.Domain.Tests.Models;

/// <summary>
/// Defines test class EmployeeDtoTests.
/// </summary>
[TestClass]
public class EmployeeDtoTests
{
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_Validate()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act

        // Assert
        Assert.IsNotNull(emp);
        Assert.IsTrue(emp.IsValid());
        Assert.AreEqual(emp.Name, "Name");
        Assert.AreEqual(emp.State, "State");
        Assert.AreEqual(emp.Country, "Country");
        Assert.AreEqual(emp.Department, EmployeeDepartmentEnum.Marketing);
        Assert.AreEqual(emp.Age, 20);
    }
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_IsValidFalse_Name()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act
        emp.Name = String.Empty;

        // Assert
        Assert.IsNotNull(emp);
        Assert.IsFalse(emp.IsValid());
    }
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_IsValidFalse_State()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act
        emp.State = String.Empty; ;

        // Assert
        Assert.IsNotNull(emp);
        Assert.IsFalse(emp.IsValid());
    }
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_IsValidFalse_Country()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act
        emp.Country = String.Empty;

        // Assert
        Assert.IsNotNull(emp);
        Assert.IsFalse(emp.IsValid());
    }
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_IsValidFalse_Age()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act
        emp.Age = 0;

        // Assert
        Assert.IsNotNull(emp);
        Assert.IsFalse(emp.IsValid());
    }
    /// <summary>
    /// Defines the test method emp_Validate.
    /// </summary>
    [TestMethod]
    public void emp_IsValidFalse_Department()
    {
        // Arrange
        var emp = GetValidEmployee();

        // Act
        emp.Department = EmployeeDepartmentEnum.Unknown;


        // Assert
        Assert.IsNotNull(emp);
        Assert.IsFalse(emp.IsValid());

    }
    public static EmployeeDto GetValidEmployee()
    {
        return new EmployeeDto(
                    999,
                    "Name",
                    20,
                    "State",
                    "Country",
                    EmployeeDepartmentEnum.Marketing);
    }

}
