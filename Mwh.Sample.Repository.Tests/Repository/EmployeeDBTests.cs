using Mwh.Sample.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Repository;

[TestClass]
public class EmployeeDBTests
{
    private EmployeeDB employeeDB;

    [TestInitialize]
    public async Task Initialize()
    {
        var builder = new DbContextOptionsBuilder();
        _ = builder.UseInMemoryDatabase("AddMultipleEmployees");
        employeeDB = new EmployeeDB(new EmployeeContext());

        var employeeService = new EmployeeDatabaseService(new EmployeeContext());
        var token = new CancellationToken();
        var employeeMock = new EmployeeMock();
        var deptResultList = new List<DepartmentResponse>();
        foreach (var dept in employeeMock.DepartmentCollection())
        {
            deptResultList.Add(await employeeService.SaveAsync(dept, token).ConfigureAwait(true));
        }
        var d = await employeeService.GetDepartmentsAsync(token).ConfigureAwait(true);
        var dcnt = d.Count();
        employeeMock.EmployeeCollection()?.ForEach(async emp =>
        {
            await employeeService.SaveAsync(emp, token).ConfigureAwait(true);
        });
        var e = await employeeService.GetEmployeesAsync(new PagingParameterModel(), token).ConfigureAwait(true);
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
    public async Task Department_Update_Id0()
    {
        // Arrange
        DepartmentDto? test = new DepartmentDto()
        {
            Id = 0,
            Name = "Test",
            Description = "Test Description"
        };
        // Act
        var result = await employeeDB.UpdateAsync(test);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, 0);
        Assert.AreNotEqual(result.Name, test.Name);
    }
    [TestMethod]
    public async Task Department_Update_MaxPlusOne()
    {
        // Arrange
        DepartmentDto? test = new DepartmentDto()
        {
            Id = 0,
            Name = "Test",
            Description = "Test Description"
        };

        // Act
        var depts = await employeeDB.DepartmentCollectionAsync();
        test.Id = depts.OrderByDescending(d => d.Id).First().Id + 1;
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
        var finalResult = await employeeDB.EmployeeAsync(result.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(addResult.Id, result.Id);
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
            Age = 22,
            Name = "Test",
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT
        };

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
        EmployeeDto emp = new EmployeeDto()
        {
            Age = 22,
            Name = "Test",
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
            Id = 98
        };

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
        Assert.IsNotNull(result);
    }
}
