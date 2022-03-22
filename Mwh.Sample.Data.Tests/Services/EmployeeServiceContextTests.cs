
using Mwh.Sample.Domain.Interfaces;

namespace Mwh.Sample.Core.Data.Tests.Services;

/// <summary>
/// 
/// </summary>
[TestClass]
public class EmployeeServiceContextTests
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
        var result = await employeeService.AddMultipleEmployeesAsync(
            namelist);

        // Assert
        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public async Task AddMultiple_StateUnderTest_Valid()
    {
        // Arrange
        string[] namelist = new string[] { "TestMultiple1", "TestMultiple2", "TestMultiple3" };

        // Act
        var initResults = await employeeService.GetAsync(cancellationToken);
        var result = await employeeService.AddMultipleEmployeesAsync(namelist);
        var afterResults = await employeeService.GetAsync(cancellationToken);
        var test = afterResults.Where(w => w.Name == "TestMultiple3").FirstOrDefault();

        // Assert
        Assert.IsNotNull(test);
        Assert.AreEqual(initResults.Count() + 3, afterResults.Count());
    }
    [TestMethod]
    public async Task DeleteAsync_ExpectedBehavior()
    {
        // Arrange
        var employee = new EmployeeModel()
        {
            Name = "TestDelete",
            Age = 20,
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartment.IT,
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
        var result = await employeeService.FindByIdAsync(id, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task Get_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeService.GetAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeService.GetAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestInitialize]
    public void Initialize()
    {
        employeeService = new EmployeeServiceContext();
        cancellationToken = default;
    }

    [TestMethod]
    public async Task Save_StateUnderTest_ValidNew()
    {
        // Arrange
        EmployeeModel item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartment.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(item, cancellationToken);

        var UpdateEmp = await employeeService.FindByIdAsync(result.Resource.id, cancellationToken).ConfigureAwait(true);

        UpdateEmp.Age = 50;

        var UpdateResult = await employeeService.SaveAsync(UpdateEmp, cancellationToken);

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
        EmployeeModel? item = null;

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
        EmployeeModel item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartment.IT,
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
        EmployeeModel item = new()
        {
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartment.IT,
        };

        // Act
        var result = await employeeService.SaveAsync(item, cancellationToken);

        var UpdateEmp = await employeeService.FindByIdAsync(result.Resource.id, cancellationToken).ConfigureAwait(true);

        UpdateEmp.Age = 50;

        var UpdateResult = await employeeService.SaveAsync(UpdateEmp, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(UpdateEmp);
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
        EmployeeModel? employee = new EmployeeModel()
        {
            id = 0,
            Name = "Test",
            Age = 25,
            State = "TX",
            Country = "USA",
            Department = EmployeeDepartment.IT
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
        EmployeeModel item = new()
        {
            id = 1,
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartment.IT,
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
        EmployeeModel item = new()
        {
            id = 999,
            Name = "Test",
            Age = 25,
            State = "Texas",
            Country = "USA",
            Department = EmployeeDepartment.IT,
        };


        var result = await employeeService.UpdateAsync(
            id,
            item,
            cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Employee Not Found", result.Message);
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
        EmployeeModel? employee = null;

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
        EmployeeModel employee = null;

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
