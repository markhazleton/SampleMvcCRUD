using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class DepartmentDtoTests
{
    [TestMethod]
    public void DepartmentDto_ConstructWithValidValues_Success()
    {
        // Arrange
        int id = 1;
        string name = "Department Name";
        string description = "Department Description";

        // Act
        var department = new DepartmentDto(id, name, description);

        var mytest = department.ToString();

        // Assert
        Assert.AreEqual("Department Id=1, Name=Department Name", mytest);
        Assert.AreEqual(id, department.Id);
        Assert.AreEqual(name, department.Name);
        Assert.AreEqual(description, department.Description);
    }

    [TestMethod]
    public void DepartmentDto_ConstructWithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        int id = 1;
        string name = "   ";

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => new DepartmentDto(id, name));
    }

    [TestMethod]
    public void DepartmentDto_ConstructWithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        int id = 0;
        string name = "Department Name";

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => new DepartmentDto(id, name));
    }

    [TestMethod]
    public void DepartmentDto_SetValidName_Success()
    {
        // Arrange
        int id = 1;
        string name = "Department Name";
        DepartmentDto department = new DepartmentDto(id, name);

        // Act
        string newName = "New Department Name";
        department.Name = newName;

        // Assert
        Assert.AreEqual(newName, department.Name);
    }

    [TestMethod]
    public void DepartmentDto_SetInvalidName_ThrowsArgumentException()
    {
        // Arrange
        int id = 1;
        string name = "Department Name";
        DepartmentDto department = new DepartmentDto(id, name);

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => department.Name = "   ");
    }

    [TestMethod]
    public void DepartmentDto_ExpectedResults()
    {
        // Arrange
        var departmentDto = new DepartmentDto(
            1,
            "Test",
            "Test")
        {
            Employees = new EmployeeDto[]
        {
            new EmployeeDto(1,
                "Test",
                22,
                "TX",
                "USA",
                EmployeeDepartmentEnum.IT),
            new EmployeeDto(
                2,
                "Test Two",
                33,
                "TX",
                "USA",
                EmployeeDepartmentEnum.IT)
        },

            // Act
            Name = "Test Name",
            Description = "Test Description"
        };

        // Assert
        Assert.IsNotNull(departmentDto);
        Assert.AreEqual(1, departmentDto.Id);
        Assert.AreEqual("Test Name", departmentDto.Name);
        Assert.AreEqual("Test Description", departmentDto.Description);
        Assert.IsNotNull(departmentDto.Employees);
        Assert.AreEqual(2, departmentDto.Employees.Length);
    }

    [TestMethod]
    public void DepartmentDto_Equality()
    {
        // Arrange
        var dept1 = new DepartmentDto(
            1,
            "Test",
            "Test");

        var dept1_copy = new DepartmentDto(
            1,
            "Test",
            "Test");

        var areEqual = (dept1 == dept1_copy);

        // Assert
        Assert.IsTrue(areEqual);
    }
    [TestMethod]
    public void DepartmentDto_Serialize()
    {
        // Arrange
        var dept1 = new DepartmentDto(
            1,
            "Test",
            "Test");

        var deptString = JsonSerializer.Serialize(dept1);

        var dept1_copy = JsonSerializer.Deserialize<DepartmentDto>(deptString);

        var areEqual = (dept1 == dept1_copy);

        // Assert
        Assert.IsTrue(areEqual);
        Assert.IsNotNull(deptString);

    }
}
