using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using Mwh.Sample.Core.WebApi.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.Tests.Controllers.Api
{
    [TestClass]
    public class EmployeeApiControllerTests
    {
        protected EmployeeService service;

        public EmployeeApiControllerTests()
        {
            EmployeeMock empMock = new EmployeeMock();
            EmployeeRepository empRepo = new EmployeeRepository(empMock);
            service = new EmployeeService(empRepo);
        }

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeApiController = new EmployeeApiController(service);
            int id = 0;

            // Act
            var result = await employeeApiController.DeleteAsync(
                id);

            // Assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public async Task FindByIdAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeApiController = new EmployeeApiController(service);
            int id = 0;

            // Act
            var result = await employeeApiController.FindByIdAsync(
                id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ListAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeApiController = new EmployeeApiController(service);

            // Act
            var result = await employeeApiController.ListAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 3);
        }

        [TestMethod]
        public async Task PostAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeApiController = new EmployeeApiController(service);
            EmployeeModel employee = null;

            // Act
            var result = await employeeApiController.PostAsync(employee);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task PutAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var employeeApiController = new EmployeeApiController(service);
            int id = 0;
            EmployeeModel employee = null;

            // Act
            var result = await employeeApiController.PutAsync(
                id,
                employee);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
