
using Mwh.Sample.Domain.Interfaces;
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
    private CancellationToken cancellationToken;
    private IEmployeeService employeeService;

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public async Task AddMultiple_StateUnderTest_Null()
    {
        // Arrange
        string[] namelist = null;

        // Act
        var result = await employeeService.AddMultipleEmployeesAsync(namelist);

        // Assert
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public async Task AddMultiple_StateUnderTest_Valid()
    {
        // Arrange
        string[] namelist = new string[] { "TestMultiple1", "TestMultiple2", "TestMultiple3" };

        // Act
        var initResults = await employeeService.GetEmployeesAsync(cancellationToken);
        var result = await employeeService.AddMultipleEmployeesAsync(namelist);
        var afterResults = await employeeService.GetEmployeesAsync(cancellationToken);
        var test = afterResults.Where(w => w.Name == "TestMultiple3").FirstOrDefault();

        // Assert
        Assert.IsNotNull(test);
        Assert.AreEqual(initResults.Count() + 3, afterResults.Count());
    }
    [TestMethod]
    public async Task DeleteAsync_ExpectedBehavior()
    {
        // Arrange
        var employee = new EmployeeDto()
        {
            Name = "TestDelete",
            Age = 20,
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(employee, cancellationToken);
        var id = result?.Resource?.id ?? 0;
        var afterResults = await employeeService.DeleteAsync(id, cancellationToken);


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
        var result = await employeeService.DeleteAsync(
            id,
            cancellationToken);

        // Assert
        Assert.AreEqual(result.Success, false);
    }

    [TestMethod]
    public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        var result = await employeeService.FindEmployeeByIdAsync(id, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task Get_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeService.GetEmployeesAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeService.GetEmployeesAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestInitialize]
    public void Initialize()
    {
        var context = new EmployeeContext();
        employeeService = new EmployeeDatabaseService(context);
        cancellationToken = default;
    }

    [TestMethod]
    public async Task Save_StateUnderTest_ValidNew()
    {
        // Arrange
        EmployeeDto item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(item, cancellationToken);

        var UpdateEmp = await employeeService.FindEmployeeByIdAsync(result.Resource.id, cancellationToken).ConfigureAwait(true);

        UpdateEmp.Resource.Age = 50;

        var UpdateResult = await employeeService.SaveAsync(UpdateEmp.Resource, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(UpdateEmp);
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
        var result = await employeeService.SaveAsync(item, cancellationToken);

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
        EmployeeDto item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(item, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }
    [TestMethod]
    public async Task SaveAsync_StateUnderTest_ValidNew()
    {
        // Arrange
        EmployeeDto item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(item, cancellationToken);
        if (result.Success)
            item = result.Resource ?? new EmployeeDto();

        var FindResult = await employeeService.FindEmployeeByIdAsync(item.id, cancellationToken).ConfigureAwait(true);

        if (FindResult.Success)
            item = FindResult.Resource ?? new EmployeeDto();

        item.Age = 50;

        var UpdateResult = await employeeService.SaveAsync(item, cancellationToken);

        if (UpdateResult.Success)
            item = UpdateResult.Resource ?? new EmployeeDto();


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
        EmployeeDto? employee = new EmployeeDto()
        {
            id = 0,
            Name = "Test",
            Age = 25,
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT
        };

        // Act
        var result = await employeeService.UpdateAsync(
            id,
            employee,
            cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Can not update employee with id(0)", result.Message);
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
        EmployeeDto item = new()
        {
            id = 1,
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };


        var result = await employeeService.UpdateAsync(
            id,
            item,
            cancellationToken);

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
        EmployeeDto item = new()
        {
            id = 999,
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartmentEnum.IT,
        };


        var result = await employeeService.UpdateAsync(
            id,
            item,
            cancellationToken);

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
        var result = await employeeService.UpdateAsync(
            id,
            employee,
            cancellationToken);

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
        EmployeeDto employee = null;

        // Act
        var result = await employeeService.UpdateAsync(
            id,
            employee,
            cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
    }
}
