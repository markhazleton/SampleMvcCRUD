namespace Mwh.Sample.Domain.Tests.Extensions;

[TestClass]
public class EnumerableExtensionsTest
{

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
