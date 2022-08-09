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
            new EmployeeDto()
            {
                Age = 10,
                Id = 1,
                Name = "Test Ten",
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartmentEnum.IT,
                DepartmentName = "IT"
            },
            new EmployeeDto()
            {
                Age = 20,
                Id = 2,
                Name = "Test Twenty",
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartmentEnum.Executive,
                DepartmentName = "Executive"
            },
            new EmployeeDto()
            {
                Age = 30,
                Id = 3,
                Name = "Test Thirty",
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartmentEnum.HR,
                DepartmentName = "HR"
            }
        };

        // Act
        var emp = myList.WithMinium(emp => emp.Age);

        // Assert
        Assert.AreEqual(emp.Age, 10);

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
