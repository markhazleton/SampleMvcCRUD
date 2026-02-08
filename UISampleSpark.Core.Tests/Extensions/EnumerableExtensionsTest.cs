using System;

namespace UISampleSpark.Core.Tests.Extensions;

[TestClass]
public class EnumerableExtensionsTest
{

    /// <summary>
    /// Test that the function returns the correct element when the sequence is non-empty and has multiple elements:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_HappyFlowInt()
    {
        List<EmployeeDto> input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Alice", Age = 30 },
            new EmployeeDto { Name = "Bob", Age = 25 },
            new EmployeeDto { Name = "Charlie", Age = 35 },
            new EmployeeDto { Name = "Sandra", Age = 27 }
        };
        EmployeeDto expectedOutput = new EmployeeDto { Name = "Bob", Age = 25 };

        EmployeeDto? resultMin = input.SelectElementByOption(emp => emp.Age);
        EmployeeDto? resultMax = input.SelectElementByOption(emp => emp.Age, EnumerableExtensions.MinMaxOption.Maximum);
        EmployeeDto? resultFirst = input.SelectElementByOption(emp => emp.Age, EnumerableExtensions.MinMaxOption.First);
        EmployeeDto? resultLast = input.SelectElementByOption(emp => emp.Age, EnumerableExtensions.MinMaxOption.Last);
        EmployeeDto? resultMean = input.SelectElementByOption(emp => emp.Age, EnumerableExtensions.MinMaxOption.Mean);

        Assert.AreEqual(expectedOutput.CompareTo(resultMin), 0);
        Assert.AreEqual(resultMax?.Name, "Charlie");
        Assert.AreEqual(resultMin?.Name, "Bob");
        Assert.AreEqual(resultLast?.Name, "Sandra");
        Assert.AreEqual(resultFirst?.Name, "Alice");
        Assert.AreEqual(resultMean?.Name, "Alice");
    }


    /// <summary>
    /// Test that the function returns the correct element when the sequence is non-empty and has multiple elements:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_HappyFlowString()
    {
        List<EmployeeDto> input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Alice", Age = 30 },
            new EmployeeDto { Name = "Bob", Age = 25 },
            new EmployeeDto { Name = "Charlie", Age = 35 }
        };
        EmployeeDto expectedOutput = new EmployeeDto { Name = "Alice", Age = 30 };
        EmployeeDto? result = input.SelectElementByOption(emp => emp.Name ?? string.Empty);
        Assert.AreEqual(expectedOutput.CompareTo(result), 0);
    }

    /// <summary>
    /// Test that the function returns the correct element when the sequence has a single element:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_SingleElement()
    {
        List<EmployeeDto> input = new List<EmployeeDto>
        {
            new EmployeeDto { Name = "Bob", Age = 25 },
        };
        EmployeeDto expectedOutput = new EmployeeDto { Name = "Bob", Age = 25 };
        EmployeeDto? result = input.SelectElementByOption(emp => emp.Age);
        Assert.AreEqual(expectedOutput.CompareTo(result), 0);
    }

    /// <summary>
    /// Test that the function returns the default value of T when the sequence is empty:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_EmptyList()
    {
        List<EmployeeDto> input = new List<EmployeeDto> { };
        EmployeeDto? result = input.SelectElementByOption(emp => emp.Age);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Test that the function returns the default value of T when the sequence is empty:
    /// </summary>
    [TestMethod]
    public void EnumerableFindMinimum_NullList()
    {
        List<EmployeeDto>? input = null;
        EmployeeDto? result = input.SelectElementByOption(emp => emp.Age);
        Assert.IsNull(result);
    }


    [TestMethod]
    public void EnumerableFindMinimum_Expected()
    {
        // Arrange
        List<EmployeeDto> myList = new List<EmployeeDto>
        {
            new EmployeeDto(
                1,
                "Test Eighteen",
                18,
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
        EmployeeDto? emp = myList.SelectElementByOption(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp?.Age, 18);

    }
    [TestMethod]
    public void EnumerableFindMinimum_Empty()
    {
        // Arrange
        List<EmployeeDto> myList = new List<EmployeeDto>();

        // Act
        EmployeeDto? emp = myList.SelectElementByOption(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp, null);

    }

    [TestMethod]
    public void EnumerableFindMinimum_Null()
    {
        // Arrange
        List<EmployeeDto>? myList = null;

        // Act
        EmployeeDto? emp = myList.SelectElementByOption(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp, null);

    }


}
