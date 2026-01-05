using Mwh.Sample.Domain.Interfaces;
using Mwh.Sample.Repository.Repository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Services;

/// <summary>
/// 
/// </summary>
[TestClass]
public class EmployeeDatabaseServiceTests
{
    private readonly CancellationToken ct;
    private readonly IEmployeeService employeeService;

    public EmployeeDatabaseServiceTests()
    {
        DbContextOptionsBuilder<EmployeeContext> builder = new DbContextOptionsBuilder<EmployeeContext>();
        _ = builder.EnableSensitiveDataLogging(true);
        _ = builder.UseInMemoryDatabase("EmployeeDatabaseServiceTests");
        EmployeeContext context = new EmployeeContext(builder.Options);
        employeeService = new EmployeeDatabaseService(context);
        ct = default;
    }


    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public async Task AddMultiple_StateUnderTest_Null()
    {
        // Arrange
        string?[]? namelist = null;

        // Act
        int result = await employeeService.AddMultipleEmployeesAsync(namelist);

        // Assert
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public async Task AddMultiple_StateUnderTest_Valid()
    {
        // Arrange
        string[] namelist = new string[] { "TestMultiple1", "TestMultiple2", "TestMultiple3" };

        // Act
        PagingParameterModel paging = new PagingParameterModel();
        int initResults = (await employeeService.GetEmployeesAsync(paging, ct).ConfigureAwait(true)).Count();
        int result = await employeeService.AddMultipleEmployeesAsync(namelist).ConfigureAwait(true);
        int afterResults = (await employeeService.GetEmployeesAsync(paging, ct).ConfigureAwait(true)).Count();
        System.Collections.Generic.IEnumerable<EmployeeDto> empList = await employeeService.GetEmployeesAsync(paging, ct).ConfigureAwait(true);
        EmployeeDto? emp1 = empList.Where(w => w.Name == namelist[0]).FirstOrDefault();

        // Assert
        Assert.IsNotNull(emp1);
        Assert.AreEqual(emp1.Name, namelist[0]);
    }
    [TestMethod]
    public async Task DeleteAsync_ExpectedBehavior()
    {
        // Arrange
        EmployeeDto employee = new EmployeeDto(
            99,
            "TestDelete",
            20,
            "TX",
            "USA",
            EmployeeDepartmentEnum.IT
        );
        employee.Gender = GenderEnum.Male;

        // Act
        EmployeeResponse? result = await employeeService.SaveAsync(employee, ct);
        int id = result?.Resource?.Id ?? 0;
        EmployeeResponse afterResults = await employeeService.DeleteAsync(id, ct);


        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(afterResults);
        Assert.IsTrue(afterResults.Success);

    }

    [TestMethod]
    public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        EmployeeResponse result = await employeeService.DeleteAsync(
            id,
            ct);

        // Assert
        Assert.AreEqual(result.Success, false);
    }

    [TestMethod]
    public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        EmployeeResponse result = await employeeService.FindEmployeeByIdAsync(id, ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task Get_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        System.Collections.Generic.IEnumerable<EmployeeDto> result = await employeeService.GetEmployeesAsync(new PagingParameterModel(), ct);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        System.Collections.Generic.IEnumerable<EmployeeDto> result = await employeeService.GetEmployeesAsync(new PagingParameterModel(), ct);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestInitialize]
    public async Task Initialize()
    {
        try
        {
            EmployeeMock employeeMock = new EmployeeMock();
            foreach (DepartmentDto dept in employeeMock.DepartmentCollection())
            {
                await employeeService.SaveAsync(dept, ct).ConfigureAwait(true);
            }
            employeeMock.EmployeeCollection()?.ForEach(async emp =>
            {
                await employeeService.UpdateAsync(emp.Id, emp, ct).ConfigureAwait(true);
            });
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
            throw;
        }
    }

    [TestMethod]
    public async Task Save_StateUnderTest_ValidNew()
    {
        // Arrange
        EmployeeDto item = new(
            999,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        EmployeeResponse result = await employeeService.SaveAsync(item, ct);

        result.Resource.Age = 50;

        EmployeeResponse UpdateResult = await employeeService.SaveAsync(result.Resource, ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result);
        Assert.IsNotNull(UpdateResult);
        Assert.IsTrue(UpdateResult.Success);
        Assert.AreEqual(50, UpdateResult.Resource?.Age);
    }

    [TestMethod]
    public async Task SaveAsync_StateUnderTest_Null()
    {
        // Arrange
        EmployeeDto? item = null;

        // Act
        EmployeeResponse result = await employeeService.SaveAsync(item, ct);

        // Assert
        Assert.IsNotNull(result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SaveAsync_StateUnderTest_Valid()
    {
        // Arrange
        CancellationToken cancellationToken = default;
        EmployeeDto item = new(
            999,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        EmployeeResponse result = await employeeService.SaveAsync(item, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }
    [TestMethod]
    public async Task SaveAsync_StateUnderTest_ValidNew()
    {
        // Arrange
        EmployeeDto item = new(
            999,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        EmployeeResponse result = await employeeService.SaveAsync(item, ct);
        if (result.Success)
            item = result.Resource;

        EmployeeResponse FindResult = await employeeService.FindEmployeeByIdAsync(item.Id, ct).ConfigureAwait(true);

        if (FindResult.Success)
            item = FindResult.Resource;

        item.Age = 50;

        EmployeeResponse UpdateResult = await employeeService.SaveAsync(item, ct);

        if (UpdateResult.Success)
            item = UpdateResult.Resource;


        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(item);
        Assert.IsNotNull(UpdateResult);
        Assert.IsTrue(UpdateResult.Success);
        Assert.AreEqual(50, UpdateResult.Resource?.Age);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_EmployeeId0()
    {
        // Arrange
        int id = 0;
        EmployeeDto item = new(
            999,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        // Act
        EmployeeResponse result = await employeeService.UpdateAsync(id, item, ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Mismatch in id(0) && id(999).", result.Message);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_LookupEmployeeId99_NoMatch()
    {
        // Arrange
        int id = 999;
        // Act
        EmployeeDto item = new(
            1,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);

        EmployeeResponse result = await employeeService.UpdateAsync(
            id,
            item,
            ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Mismatch in id(999) && id(1).", result.Message);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_LookupEmployeeId999_NotFound()
    {
        // Arrange
        int id = 999;
        // Act
        EmployeeDto item = new(
            999,
            "Test",
            25,
            "Texas",
            "USA",
            EmployeeDepartmentEnum.IT);


        EmployeeResponse result = await employeeService.UpdateAsync(
            id,
            item,
            ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(string.Empty, result.Message);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_NullEmployeeId99()
    {
        // Arrange
        int id = 99;
        EmployeeDto? employee = null;

        // Act
        EmployeeResponse result = await employeeService.UpdateAsync(
            id,
            employee,
            ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Can not update null employee", result.Message);
    }
    [TestMethod]
    public async Task UpdateAsync_NullEmployeeIdZero()
    {
        // Arrange
        int id = 0;
        EmployeeDto? employee = null;

        // Act
        EmployeeResponse result = await employeeService.UpdateAsync(
            id,
            employee,
            ct);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
    }
}
