using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.Data.Tests.Services
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private EmployeeService service;
        private CancellationToken token;

        [TestInitialize]
        public void Initialize()
        {
            service = new EmployeeService();
            token = default;
        }


        [TestMethod]
        public void AddMultipleEmployees_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            string[] namelist = null;

            // Act
            var result = service.AddMultipleEmployees(
                namelist);

            // Assert
            Assert.AreEqual(-1,result);
        }

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await service.DeleteAsync(
                id,
                token);

            // Assert
            Assert.AreEqual(result.Success,false);
        }

        [TestMethod]
        public void Dispose_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            service.Dispose();

            // Assert
        }

        [TestMethod]
        public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await service.FindByIdAsync(id,token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);

        }

        [TestMethod]
        public void GetEmployeeCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = service.GetEmployeeCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ListAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = await service.ListAsync(
                token);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SaveAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel employee = null;

            // Act
            var result = await service.SaveAsync(
                employee,
                token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);

        }

        [TestMethod]
        public void SaveEmployee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel item = null;

            // Act
            var result = service.SaveEmployee(
                item);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SaveEmployeeAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = new EmployeeService();
            EmployeeModel item = null;

            // Act
            var result = await service.SaveEmployeeAsync(
                item);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior()
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
            Assert.IsFalse(result.Success);
        }
    }
}
