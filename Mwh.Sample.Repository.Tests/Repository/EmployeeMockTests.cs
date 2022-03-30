using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Repository.Repository;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Repository
{
    [TestClass]
    public class EmployeeMockTests
    {
        [TestMethod]
        public async Task DepartmentAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int id = 0;

            // Act
            var result = await employeeMock.DepartmentAsync(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EmployeeAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int id = 0;

            // Act
            var result = await employeeMock.EmployeeAsync(
                id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            var result = employeeMock.Delete(
                ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            var result = await employeeMock.DeleteEmployeeAsync(
                ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DepartmentCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();

            // Act
            var result = employeeMock.DepartmentCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DepartmentCollectionAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();

            // Act
            var result = await employeeMock.DepartmentCollectionAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EmployeeCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();

            // Act
            var result = employeeMock.EmployeeCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EmployeeCollectionAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();

            // Act
            var result = await employeeMock.EmployeeCollectionAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            EmployeeDto emp = null;

            // Act
            var result = await employeeMock.UpdateAsync(
                emp);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            DepartmentDto emp = null;

            // Act
            var result = await employeeMock.UpdateAsync(emp);

            // Assert
            Assert.IsNull(result);
        }
    }
}
