using Mwh.Sample.Repository.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Repository;

[TestClass]
public class EmployeeDBTests
{
    private readonly EmployeeDB employeeDB;
    public EmployeeDBTests()
    {
        var builder = new DbContextOptionsBuilder();
        _ = builder.EnableSensitiveDataLogging(true);
        _ = builder.UseInMemoryDatabase("EmployeeDBTests");
        employeeDB = new EmployeeDB(new EmployeeContext(builder.Options));
    }

    [TestInitialize]
    public async Task Initialize()
    {
        try
        {
            var employeeMock = new EmployeeMock();
            foreach (var dept in employeeMock.DepartmentCollection())
            {
                await employeeDB.UpdateAsync(dept).ConfigureAwait(true);
            }
            employeeMock.EmployeeCollection()?.ForEach(async emp =>
            {
                await employeeDB.UpdateAsync(emp).ConfigureAwait(true);
            });
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            throw;
        }

    }
    [TestMethod]
    public async Task Department_List_Expected()
    {
        // Arrange

        // Act
        var result = await employeeDB.DepartmentCollectionAsync();

        // Assert
        Assert.IsNotNull(result);
    }
    [TestMethod]
    public async Task Department_iD1_Expected()
    {
        // Arrange
        int DeptId = 1;
        // Act
        var result = await employeeDB.DepartmentAsync(DeptId);

        // Assert
        Assert.IsNotNull(result);
    }
    [TestMethod]
    public async Task Department_iD1_Update()
    {
        // Arrange
        int DeptId = 1;
        // Act
        var result = await employeeDB.DepartmentAsync(DeptId);

        result.Description = "Test Description";
        result.Name = "Test Name";
        var result2 = await employeeDB.UpdateAsync(result);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result2.Name, "Test Name");
        Assert.AreEqual(result2.Description, "Test Description");
    }
    [TestMethod]
    public async Task Department_Update_Null()
    {
        // Arrange
        DepartmentDto? test = null;
        // Act
        var result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Department_Update_Id1()
    {
        // Arrange
        DepartmentDto test = new DepartmentDto(1, "Test", "Test Description");

        // Act
        var result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, 1);
        Assert.AreEqual(result.Name, test.Name);
    }
    [TestMethod]
    public async Task Department_Update_MaxPlusOne()
    {
        // Arrange
        var depts = await employeeDB.DepartmentCollectionAsync();
        DepartmentDto? test = new(depts.OrderByDescending(d => d.Id).First().Id, "Test", "Test Description");

        // Act
        var result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, test.Id);
        Assert.AreEqual(result.Name, test.Name);
        Assert.AreEqual(result.Description, test.Description);
    }

    [TestMethod]
    public async Task Delete_StateUnderTest_ExpectedBehaviorNewEmployee()
    {
        // Arrange
        var newEmp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act

        // Get Current count of employees
        var initResult = await employeeDB.EmployeeCollectionAsync();

        // Add New Employee with Update
        var addResult = await employeeDB.UpdateAsync(newEmp);
        // Get updated count of employees
        var updatedResult = await employeeDB.EmployeeCollectionAsync();
        /// Delete the Employee
        var result = await employeeDB.DeleteEmployeeAsync(addResult.Id);
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
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Employee_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        var result = await employeeDB.EmployeeAsync(id);

        // Assert
        Assert.IsNull(result);
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
        var newEmp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

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
        var finalResult = await employeeDB.EmployeeAsync(result.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(addResult.Id, result.Id);
        Assert.AreEqual(initResult.Count, updatedResult.Count);
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
        var emp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id > 0);
    }
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public async Task Update_NewEmployeeWithId98()
    {
        // Arrange
        var emp = new EmployeeDto(
            98,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);


        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(98, result.Id);
    }

    [TestMethod]
    public async Task Update_NullEmployee()
    {
        // Arrange
        EmployeeDto? emp = null;

        // Act
        var result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNull(result);
    }
}
