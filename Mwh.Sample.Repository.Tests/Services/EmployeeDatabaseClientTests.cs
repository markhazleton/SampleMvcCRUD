using Mwh.Sample.Domain.Interfaces;

namespace Mwh.Sample.Repository.Tests.Services;

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
        var serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.AddMultipleEmployeesAsync(namelist)).ReturnsAsync(2);
        var client = new EmployeeDatabaseClient(serviceMock.Object);

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
        var cancellationToken = new CancellationToken();
        var response = new EmployeeResponse();
        var serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.DeleteAsync(id, cancellationToken)).ReturnsAsync(response);
        var client = new EmployeeDatabaseClient(serviceMock.Object);

        // Act
        var result = await client.DeleteAsync(id, cancellationToken);

        // Assert
        Assert.AreEqual(response, result);
        serviceMock.Verify(s => s.DeleteAsync(id, cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task FindDepartmentByIdAsync_ShouldCallServiceMethod()
    {
        // Arrange
        int departmentId = (int)EmployeeDepartmentEnum.IT;
        var cancellationToken = new CancellationToken();
        var departmentDto = new DepartmentDto(EmployeeDepartmentEnum.IT);
        var serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.FindDepartmentByIdAsync(departmentId, cancellationToken)).ReturnsAsync(departmentDto);
        var client = new EmployeeDatabaseClient(serviceMock.Object);

        // Act
        var result = await client.FindDepartmentByIdAsync(departmentId, cancellationToken);

        // Assert
        Assert.AreEqual(departmentDto, result);
        serviceMock.Verify(s => s.FindDepartmentByIdAsync(departmentId, cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task FindEmployeeByIdAsync_ShouldCallServiceMethod()
    {
        // Arrange
        int employeeId = 1;
        var cancellationToken = new CancellationToken();
        var employee = new EmployeeDto(employeeId, "Test", 20, "Texas", "USA", EmployeeDepartmentEnum.IT);
        var employeeResponse = new EmployeeResponse(employee);
        var serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.FindEmployeeByIdAsync(employeeId, cancellationToken)).ReturnsAsync(employeeResponse);
        var client = new EmployeeDatabaseClient(serviceMock.Object);

        // Act
        var result = await client.FindEmployeeByIdAsync(employeeId, cancellationToken);

        // Assert
        Assert.AreEqual(employeeResponse, result);
        serviceMock.Verify(s => s.FindEmployeeByIdAsync(employeeId, cancellationToken), Times.Once);
    }
    [TestMethod]
    public async Task GetDepartmentsAsync_ShouldCallServiceMethod()
    {
        // Arrange
        bool includeEmployees = true;
        var cancellationToken = new CancellationToken();
        var departmentDtos = new List<DepartmentDto>
        {
            new DepartmentDto(EmployeeDepartmentEnum.IT),
            new DepartmentDto(EmployeeDepartmentEnum.HR)
        };
        var serviceMock = new Mock<IEmployeeService>();
        serviceMock.Setup(s => s.GetDepartmentsAsync(includeEmployees, cancellationToken)).ReturnsAsync(departmentDtos);
        var client = new EmployeeDatabaseClient(serviceMock.Object);

        // Act
        var result = await client.GetDepartmentsAsync(includeEmployees, cancellationToken);

        // Assert
        Assert.AreEqual(departmentDtos, result);
        serviceMock.Verify(s => s.GetDepartmentsAsync(includeEmployees, cancellationToken), Times.Once);
    }

}
