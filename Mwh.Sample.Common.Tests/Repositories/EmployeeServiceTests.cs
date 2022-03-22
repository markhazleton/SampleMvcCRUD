
namespace Mwh.Sample.Common.Tests.Repositories;

/// <summary>
/// 
/// </summary>
[TestClass]
public class EmployeeServiceTests
{
    private EmployeeService service;
    private CancellationToken token;

    /// <summary>
    /// 
    /// </summary>
    public EmployeeServiceTests()
    {
        service = new EmployeeService(new EmployeeRepository(new EmployeeMock()));
        token = default;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;
        CancellationToken token = default(CancellationToken);

        // Act
        var result = await service.DeleteAsync(id, token);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        var result = await service.FindByIdAsync(
            id,
            token);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        service = new EmployeeService(new EmployeeRepository(new EmployeeMock()));
        token = default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task ListAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await service.GetAsync(token);

        // Assert
        Assert.IsNotNull(result);
    }
    /// <summary>
    /// SaveAsync_NullEmployee
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SaveAsync_Employee_ExpectedBehavior()
    {
        // Arrange
        EmployeeModel? employee = new EmployeeModel()
        {
            Age = 33,
            Country = "USA",
            State = "TX",
            Name = "TEST",
            Department = EmployeeDepartment.IT,
            id = 999
        };

        // Act
        var result = await service.SaveAsync(employee, token);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// SaveAsync_NullEmployee
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SaveAsync_NullEmployee_ExpectedBehavior()
    {
        // Arrange
        EmployeeModel? employee = null;

        // Act
        var result = await service.SaveAsync(
            employee,
            token);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// UpdateAsync_Employee
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_Employee_ExpectedBehavior()
    {

        // Act
        var mockEmployee = new EmployeeMock();

        EmployeeModel? employee = mockEmployee.EmployeeCollection().FirstOrDefault();

        // Arrange
        int id = employee?.id ?? 0;
        employee.Name = "Update";

        var result = await service.UpdateAsync(
            id,
            employee,
            token);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Resource.Name, employee.Name);
    }

    /// <summary>
    /// UpdateAsync_NullEmployee
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task UpdateAsync_NullEmployee_ExpectedBehavior()
    {
        // Arrange
        int id = 0;
        EmployeeModel employee = null;

        // Act
        var result = await service.UpdateAsync(
            id,
            employee,
            token);

        // Assert
        Assert.IsNotNull(result);
    }
}
