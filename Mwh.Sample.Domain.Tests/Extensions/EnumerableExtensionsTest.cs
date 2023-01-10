using System;

namespace Mwh.Sample.Domain.Tests.Extensions;

[TestClass]
public class EnumerableExtensionsTest
{

    /// <summary>
    /// Test that the function returns the correct element when the sequence is non-empty and has multiple elements:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_HappyFlowInt()
    {
        var input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Alice", Age = 30 },
            new EmployeeDto { Name = "Bob", Age = 25 },
            new EmployeeDto { Name = "Charlie", Age = 35 }
        };
        var expectedOutput = new EmployeeDto { Name = "Bob", Age = 25 };
        var result = input.WithMinium(emp => emp.Age);
        Assert.AreEqual(expectedOutput.CompareTo(result), 0);
    }


    /// <summary>
    /// Test that the function returns the correct element when the sequence is non-empty and has multiple elements:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_HappyFlowString()
    {
        var input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Alice", Age = 30 },
            new EmployeeDto { Name = "Bob", Age = 25 },
            new EmployeeDto { Name = "Charlie", Age = 35 }
        };
        var expectedOutput = new EmployeeDto { Name = "Alice", Age = 30 };
        var result = input.WithMinium(emp => emp.Name);
        Assert.AreEqual(expectedOutput.CompareTo(result), 0);
    }

    /// <summary>
    /// Test that the function returns the correct element when the sequence has a single element:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_SingleElement()
    {
        var input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Bob", Age = 25 },
        };
        var expectedOutput = new EmployeeDto { Name = "Bob", Age = 25 };
        var result = input.WithMinium(emp => emp.Age);
        Assert.AreEqual(expectedOutput.CompareTo(result), 0);
    }

    /// <summary>
    /// Test that the function returns the default value of T when the sequence is empty:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_EmptyList()
    {
        var input = new List<EmployeeDto> { };
        var result = input.WithMinium(emp => emp.Age);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Test that the function returns the default value of T when the sequence is empty:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_NullList()
    {
        List<EmployeeDto>? input = null;
        var result = input.WithMinium(emp => emp.Age);
        Assert.IsNull(result);
    }


    [TestMethod]
    public void EnumerableFindMinimum_Expected()
    {
        // Arrange
        var myList = new List<EmployeeDto>
        {
            new EmployeeDto(
                1,
                "Test Ten",
                10,
                "Texas",
                "USA",
                EmployeeDepartmentEnum.IT
            ),
            new EmployeeDto(
                2,
                "Test Twenty",
                20,
                "Texas",
                "USA",
                EmployeeDepartmentEnum.Executive),
            new EmployeeDto(
                3,
                "Test Thirty",
                30,
                "Texas",
                "USA",
                EmployeeDepartmentEnum.HR)
        };

        // Act
        var emp = myList.WithMinium(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp?.Age, 10);

    }
    [TestMethod]
    public void EnumerableFindMinimum_Empty()
    {
        // Arrange
        List<EmployeeDto> myList = new List<EmployeeDto>();

        // Act
        var emp = myList.WithMinium(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp, null);

    }

    [TestMethod]
    public void EnumerableFindMinimum_Null()
    {
        // Arrange
        List<EmployeeDto>? myList = null;

        // Act
        var emp = myList.WithMinium(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp, null);

    }


}
