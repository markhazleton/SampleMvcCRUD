
namespace Mwh.Sample.Common.Tests.Repositories;

[TestClass]
public class EmployeeRepositoryTests
{
    private CancellationToken token;
    private EmployeeModel employee;
    private EmployeeMock mockEmployee;
    private EmployeeRepository employeeRepository;

    [TestInitialize]
    public void Initialize()
    {
        mockEmployee = new EmployeeMock();
        employeeRepository = new EmployeeRepository(mockEmployee);
        employee = new EmployeeModel();
        token = default(global::System.Threading.CancellationToken);
    }

    [TestMethod]
    public async Task AddAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var emp = await employeeRepository.AddAsync(employee, token);

        // Assert
        Assert.IsNotNull(emp);
    }

    [TestMethod]
    public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        int id = 0;

        // Act
        var result = await employeeRepository.FindByIdAsync(
            id,
            token);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ListAsync_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeRepository.ListAsync(token);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Remove_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeRepository.RemoveAsync(
            employee,
            token);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Update_StateUnderTest_ExpectedBehavior()
    {
        // Arrange

        // Act
        var result = await employeeRepository.UpdateAsync(
            employee,
            token);

        // Assert
        Assert.IsNotNull(result);
    }
}
