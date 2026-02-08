using UISampleSpark.Core.Interfaces;

namespace UISampleSpark.Data.Tests.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public class EmployeeDatabaseClientTests
{
    [TestMethod]
    public async Task AddMultipleEmployeesAsync_ShouldCallServiceMethod()
    {
        // Arrange
        string[] namelist = { "John", "Jane" };
        Mock<IEmployeeService> serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.AddMultipleEmployeesAsync(namelist)).ReturnsAsync(2);
        EmployeeDatabaseClient client = new EmployeeDatabaseClient(serviceMock.Object, NullLogger<EmployeeDatabaseClient>.Instance);

        // Act
        int result = await client.AddMultipleEmployeesAsync(namelist);

        // Assert
        Assert.AreEqual(2, result);
        serviceMock.Verify(s => s.AddMultipleEmployeesAsync(namelist), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldCallServiceMethod()
    {
        // Arrange
        int id = 1;
        CancellationToken cancellationToken = new CancellationToken();
        EmployeeResponse response = new EmployeeResponse();
        Mock<IEmployeeService> serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.DeleteAsync(id, cancellationToken)).ReturnsAsync(response);
        EmployeeDatabaseClient client = new EmployeeDatabaseClient(serviceMock.Object, NullLogger<EmployeeDatabaseClient>.Instance);

        // Act
        EmployeeResponse result = await client.DeleteAsync(id, cancellationToken);

        // Assert
        Assert.AreEqual(response, result);
        serviceMock.Verify(s => s.DeleteAsync(id, cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task FindDepartmentByIdAsync_ShouldCallServiceMethod()
    {
        // Arrange
        int departmentId = (int)EmployeeDepartmentEnum.IT;
        CancellationToken cancellationToken = new CancellationToken();
        DepartmentDto departmentDto = new DepartmentDto(EmployeeDepartmentEnum.IT);
        Mock<IEmployeeService> serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.FindDepartmentByIdAsync(departmentId, cancellationToken)).ReturnsAsync(departmentDto);
        EmployeeDatabaseClient client = new EmployeeDatabaseClient(serviceMock.Object, NullLogger<EmployeeDatabaseClient>.Instance);

        // Act
        DepartmentDto result = await client.FindDepartmentByIdAsync(departmentId, cancellationToken);

        // Assert
        Assert.AreEqual(departmentDto, result);
        serviceMock.Verify(s => s.FindDepartmentByIdAsync(departmentId, cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task FindEmployeeByIdAsync_ShouldCallServiceMethod()
    {
        // Arrange
        int employeeId = 1;
        CancellationToken cancellationToken = new CancellationToken();
        EmployeeDto employee = new EmployeeDto(employeeId, "Test", 20, "Texas", "USA", EmployeeDepartmentEnum.IT);
        EmployeeResponse employeeResponse = new EmployeeResponse(employee);
        Mock<IEmployeeService> serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.FindEmployeeByIdAsync(employeeId, cancellationToken)).ReturnsAsync(employeeResponse);
        EmployeeDatabaseClient client = new EmployeeDatabaseClient(serviceMock.Object, NullLogger<EmployeeDatabaseClient>.Instance);

        // Act
        EmployeeResponse result = await client.FindEmployeeByIdAsync(employeeId, cancellationToken);

        // Assert
        Assert.AreEqual(employeeResponse, result);
        serviceMock.Verify(s => s.FindEmployeeByIdAsync(employeeId, cancellationToken), Times.Once);
    }
    [TestMethod]
    public async Task GetDepartmentsAsync_ShouldCallServiceMethod()
    {
        // Arrange
        bool includeEmployees = true;
        CancellationToken cancellationToken = new CancellationToken();
        List<DepartmentDto> departmentDtos = new List<DepartmentDto>
        {
            new DepartmentDto(EmployeeDepartmentEnum.IT),
            new DepartmentDto(EmployeeDepartmentEnum.HR)
        };
        Mock<IEmployeeService> serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.GetDepartmentsAsync(includeEmployees, cancellationToken)).ReturnsAsync(departmentDtos);
        EmployeeDatabaseClient client = new EmployeeDatabaseClient(serviceMock.Object, NullLogger<EmployeeDatabaseClient>.Instance);

        // Act
        IEnumerable<DepartmentDto> result = await client.GetDepartmentsAsync(includeEmployees, cancellationToken);

        // Assert
        Assert.AreEqual(departmentDtos, result);
        serviceMock.Verify(s => s.GetDepartmentsAsync(includeEmployees, cancellationToken), Times.Once);
    }

}


