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
        DbContextOptionsBuilder<EmployeeContext> builder = new DbContextOptionsBuilder<EmployeeContext>();
        _ = builder.EnableSensitiveDataLogging(true);
        _ = builder.UseInMemoryDatabase("EmployeeDBTests");
        employeeDB = new EmployeeDB(new EmployeeContext(builder.Options));
    }

    [TestInitialize]
    public async Task Initialize()
    {
        try
        {
            EmployeeMock employeeMock = new EmployeeMock();
            foreach (DepartmentDto dept in employeeMock.DepartmentCollection())
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
        System.Collections.Generic.List<DepartmentDto> result = await employeeDB.DepartmentCollectionAsync();

        // Assert
        Assert.IsNotNull(result);
    }
    [TestMethod]
    public async Task Department_iD1_Expected()
    {
        // Arrange
        int DeptId = 1;
        // Act
        DepartmentDto result = await employeeDB.DepartmentAsync(DeptId);

        // Assert
        Assert.IsNotNull(result);
    }
    [TestMethod]
    public async Task Department_iD1_Update()
    {
        // Arrange
        int DeptId = 1;
        // Act
        DepartmentDto result = await employeeDB.DepartmentAsync(DeptId);

        result.Description = "Test Description";
        result.Name = "Test Name";
        DepartmentDto? result2 = await employeeDB.UpdateAsync(result);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result2);
        Assert.AreEqual(result2.Name, "HR");
        Assert.AreEqual(result2.Description, "HR");
    }
    [TestMethod]
    public async Task Department_Update_Null()
    {
        // Arrange
        DepartmentDto? test = null;
        // Act
        DepartmentDto? result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Department_Update_Id1()
    {
        // Arrange
        DepartmentDto test = new()
        {
            Id = 1,
            Name = "HR",
            Description = "HR"
        };

        // Act
        DepartmentDto? result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, 1);
        Assert.AreEqual(result.Name, test.Name);
    }
    [TestMethod]
    public async Task Department_Update_MaxPlusOne()
    {
        // Arrange
        System.Collections.Generic.List<DepartmentDto> depts = await employeeDB.DepartmentCollectionAsync();
        DepartmentDto dept = depts.OrderByDescending(d => d.Id).First();

        DepartmentDto? test = new((EmployeeDepartmentEnum)dept.Id);

        // Act
        DepartmentDto? result = await employeeDB.UpdateAsync(test);
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
        EmployeeDto newEmp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act

        // Get Current count of employees
        System.Collections.Generic.List<EmployeeDto> initResult = await employeeDB.EmployeeCollectionAsync();

        // Add New Employee with Update
        EmployeeDto? addResult = await employeeDB.UpdateAsync(newEmp);
        // Get updated count of employees
        System.Collections.Generic.List<EmployeeDto> updatedResult = await employeeDB.EmployeeCollectionAsync();
        /// Delete the Employee
        bool result = await employeeDB.DeleteEmployeeAsync(addResult.Id);
        // Get result after delete
        System.Collections.Generic.List<EmployeeDto> finalResult = await employeeDB.EmployeeCollectionAsync();

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
        bool result = await employeeDB.DeleteEmployeeAsync(ID);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Employee_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        EmployeeDto? result = await employeeDB.EmployeeAsync(id);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task EmployeeCollection_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        System.Collections.Generic.List<EmployeeDto> result = await employeeDB.EmployeeCollectionAsync();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Update_StateUnderTest_ExpectedBehaviorNewEmployee()
    {
        // Arrange
        EmployeeDto newEmp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act

        // Get Current count of employees
        System.Collections.Generic.List<EmployeeDto> initResult = await employeeDB.EmployeeCollectionAsync();

        // Add New Employee with Update
        EmployeeDto? addResult = await employeeDB.UpdateAsync(newEmp);

        // Get updated count of employees
        System.Collections.Generic.List<EmployeeDto> updatedResult = await employeeDB.EmployeeCollectionAsync();
        /// Update the Employee
        addResult.Name = "Test User 2";
        addResult.Age = 44;
        addResult.State = "FL";
        addResult.Department = EmployeeDepartmentEnum.Accounting;
        EmployeeDto? result = await employeeDB.UpdateAsync(addResult);
        // Get result after update
        EmployeeDto? finalResult = await employeeDB.EmployeeAsync(result.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(addResult.Id, result.Id);
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
        EmployeeDto emp = new EmployeeDto(
            99,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        EmployeeDto? result = await employeeDB.UpdateAsync(emp);

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
        EmployeeDto emp = new EmployeeDto(
            98,
            "Test User",
            33,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);


        // Act
        EmployeeDto? result = await employeeDB.UpdateAsync(emp);

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
        EmployeeDto? result = await employeeDB.UpdateAsync(emp);

        // Assert
        Assert.IsNull(result);
    }
}
