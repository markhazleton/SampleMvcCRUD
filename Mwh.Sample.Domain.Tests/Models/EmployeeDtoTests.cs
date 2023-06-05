
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

    [TestMethod]
    public void EmployeeDto_Equals_DifferentProperties_ReturnsFalse()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(2, "Jane Smith", 25, "New York", "USA", EmployeeDepartmentEnum.Marketing);

        // Act
        var areEqual = employee1.Equals(employee2);

        // Assert
        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void EmployeeDto_Equals_SameProperties_ReturnsTrue()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var areEqual = employee1.Equals(employee2);

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void EmployeeDto_InvalidData_IsNotValid()
    {
        // Arrange
        EmployeeDto employee = new()
        {
            Name = string.Empty,
            Age = 0,
            Country = string.Empty,
            Department = EmployeeDepartmentEnum.Unknown,
            Id = 0
        };

        // Act
        var isValid = employee.IsValid();

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void EmployeeDto_InvalidData_IsNotValid_new()
    {
        // Arrange
        var employee = new EmployeeDto();

        // Act
        var isValid = employee.IsValid();

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void EmployeeDto_InvalidData_ThrowsArgumentException()
    {
        // Arrange
        int id = 1;
        string name = string.Empty; // Invalid empty name
        int age = 30;
        string state = "California";
        string country = "USA";
        EmployeeDepartmentEnum department = EmployeeDepartmentEnum.IT;

        // Act and Assert
        Assert.ThrowsException<EmployeeDtoValidationException>(() => new EmployeeDto(id, name, age, state, country, department));
    }


    [TestMethod]
    public void EmployeeDto_ValidData_IsValid()
    {
        // Arrange
        var employee = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var isValid = employee.IsValid();

        // Assert
        Assert.IsTrue(isValid);
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


    [TestMethod]
    public void GetHashCode_EqualProperties_ReturnsSameHashCode()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var hashCode1 = employee1.GetHashCode();
        var hashCode2 = employee2.GetHashCode();

        // Assert
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void GetHashCode_DifferentProperties_ReturnsDifferentHashCode()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(2, "Jane Smith", 25, "New York", "USA", EmployeeDepartmentEnum.Accounting);

        // Act
        var hashCode1 = employee1.GetHashCode();
        var hashCode2 = employee2.GetHashCode();

        // Assert
        Assert.AreNotEqual(hashCode1, hashCode2);
    }

    [TestMethod]
    public void Equals_SameInstance_ReturnsTrue()
    {
        // Arrange
        var employee = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var result = employee.Equals(employee);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equals_EqualProperties_ReturnsTrue()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var result = employee1.Equals(employee2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equals_DifferentProperties_ReturnsFalse()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(2, "Jane Smith", 25, "New York", "USA", EmployeeDepartmentEnum.Accounting);

        // Act
        var result = employee1.Equals(employee2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_NullProperties_ReturnsFalse()
    {
        // Arrange
        var employee1 = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var employee2 = new EmployeeDto(1, "Jane Smith", 30, "California", "USA", EmployeeDepartmentEnum.IT)
        {
            Name = null
        };

        // Act
        var result = employee1.Equals(employee2);
        var result2 = employee2.Equals(employee1);

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }

    [TestMethod]
    public void Equals_NullObject_ReturnsFalse()
    {
        // Arrange
        var employee = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);

        // Act
        var result = employee.Equals(null);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_DifferentObjectType_ReturnsFalse()
    {
        // Arrange
        var employee = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        var otherObject = new object();

        // Act
        var result = employee.Equals(otherObject);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_BothInstancesNull_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = null;
        EmployeeDto employee2 = null;

        // Act
        var result = Equals(employee1, employee2);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public void Equals_SingleInstanceNull_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = null;

        // Act
        var result = Equals(employee1, employee1);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public void Equals_OneInstancesNull_ReturnsFalse()
    {
        // Arrange
        var employee = new EmployeeDto(1, "John Doe", 30, "California", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = null;

        // Act
        var result = employee.Equals(employee2);
        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CompareTo_NullObject_Returns1()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto();
        EmployeeDto employee2 = null;

        // Act
        int result = employee1.CompareTo(employee2);

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void CompareTo_EqualObjects_Returns0()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        int result = employee1.CompareTo(employee2);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CompareTo_DifferentId_ReturnsNonZero()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        int result = employee1.CompareTo(employee2);

        // Assert
        Assert.IsTrue(result != 0);
    }

    [TestMethod]
    public void CompareTo_DifferentName_ReturnsNonZero()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "Jane", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        employee2.Name = null;

        // Act
        int result = employee1.CompareTo(employee2);
        int result2 = employee2.CompareTo(employee1);

        // Assert
        Assert.IsTrue(result != 0);
        Assert.IsTrue(result2 != 0);
    }

    [TestMethod]
    public void CompareTo_DifferentAge_ReturnsNonZero()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 35, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        int result = employee1.CompareTo(employee2);

        // Assert
        Assert.IsTrue(result != 0);
    }

    [TestMethod]
    public void OperatorEquals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 == employee2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void OperatorEquals_DifferentObjects_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 == employee2;

        // Assert
        Assert.IsFalse(result);
    }

    // Similar tests for the remaining operator overloads: !=, <, <=, >, >=

    [TestMethod]
    public void OperatorNotEquals_EqualObjects_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 != employee2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentObjects_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 != employee2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void OperatorGreaterThan_GreaterThan_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee2 > employee1;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void OperatorGreaterThan_LessThan_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 > employee2;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void OperatorGreaterThan_EqualObjects_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 > employee2;

        // Assert
        Assert.IsFalse(result);
    }

    // Similar tests for the < (less than), >= (greater than or equal to), <= (less than or equal to) operator overloads

    [TestMethod]
    public void OperatorLessThan_GreaterThan_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee2 < employee1;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void OperatorLessThan_LessThan_ReturnsTrue()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(2, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 < employee2;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void OperatorLessThan_EqualObjects_ReturnsFalse()
    {
        // Arrange
        EmployeeDto employee1 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);
        EmployeeDto employee2 = new EmployeeDto(1, "John", 30, "CA", "USA", EmployeeDepartmentEnum.IT);

        // Act
        bool result = employee1 < employee2;

        // Assert
        Assert.IsFalse(result);
    }


}
