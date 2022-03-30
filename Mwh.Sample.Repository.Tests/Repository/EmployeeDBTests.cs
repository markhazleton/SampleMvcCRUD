using Mwh.Sample.Repository.Repository;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Repository;

[TestClass]
public class EmployeeDBTests
{
    private EmployeeDB employeeDB;

    [TestInitialize]
    public void Initialize()
    {
        var builder = new DbContextOptionsBuilder();
        _ = builder.UseInMemoryDatabase("AddMultipleEmployees");
        employeeDB = new EmployeeDB(new EmployeeContext());





    }


    [TestMethod]
    public async Task Delete_StateUnderTest_ExpectedBehaviorNewEmployee()
    {
        // Arrange
        var newEmp = new EmployeeDto()
        {
            Age = 33,
            Name = "Test User",
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT
        };

        // Act

        // Get Current count of employees
        var initResult = await employeeDB.EmployeeCollectionAsync();

        // Add New Employee with Update
        var addResult = await employeeDB.UpdateAsync(newEmp);
        // Get updated count of employees
        var updatedResult = await employeeDB.EmployeeCollectionAsync();
        /// Delete the Employee
        var result = await employeeDB.DeleteEmployeeAsync(addResult.id);
        // Get result after delete
        var finalResult = await employeeDB.EmployeeCollectionAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(finalResult.Count, initResult.Count);

    }


    [TestMethod]
    public async Task Delete_StateUnderTest_ExpectedBehaviorNotFound()
    {
        // Arrange
        int ID = 0;

        // Act
        var result = await employeeDB.DeleteEmployeeAsync(ID);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Employee_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        var result = await employeeDB.EmployeeAsync(id);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task EmployeeCollection_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeDB.EmployeeCollectionAsync();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Update_StateUnderTest_ExpectedBehaviorNewEmployee()
    {
        // Arrange
        var newEmp = new EmployeeDto()
        {
            Age = 33,
            Name = "Test User",
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT
        };

        // Act

        // Get Current count of employees
        var initResult = await employeeDB.EmployeeCollectionAsync();

        // Add New Employee with Update
        var addResult = await employeeDB.UpdateAsync(newEmp);

        // Get updated count of employees
        var updatedResult = await employeeDB.EmployeeCollectionAsync();
        /// Update the Employee
        addResult.Name = "Test User 2";
        addResult.Age = 44;
        addResult.State = "FL";
        addResult.Department = EmployeeDepartmentEnum.Accounting;
        var result = await employeeDB.UpdateAsync(addResult);
        // Get result after update
        var finalResult = await employeeDB.EmployeeAsync(result.id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(addResult.id, result.id);
        Assert.AreNotEqual(initResult.Count, updatedResult.Count);
        Assert.AreEqual(finalResult.Age, 44);
        Assert.AreEqual(finalResult.State, "FL");

    }
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public async Task Update_NewEmployee()
    {
        // Arrange
        EmployeeDto emp = new EmployeeDto()
        { 
            Age =22,
            Name ="Test",
            State = "TX",
            Country="USA",
            Department = EmployeeDepartmentEnum.IT
        };

        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.id>0);
    }
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public async Task Update_NewEmployeeWithId98()
    {
        // Arrange
        EmployeeDto emp = new EmployeeDto()
        {
            Age = 22,
            Name = "Test",
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
            id=98
        };

        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(98,result.id);
    }

    [TestMethod]
    public async Task Update_NullEmployee()
    {
        // Arrange
        EmployeeDto emp = null;

        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNotNull(result);
    }
}
