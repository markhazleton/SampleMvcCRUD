using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Tests.Repositories
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private EmployeeService service;
        private CancellationToken token;

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;
            CancellationToken token = default(global::System.Threading.CancellationToken);

            // Act
            var result = await service.DeleteAsync(id, token);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

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

        [TestInitialize]
        public void Initialize()
        {
            service = new EmployeeService(new EmployeeRepository(new EmployeeMock()));
            token = default(global::System.Threading.CancellationToken);
        }

        [TestMethod]
        public async Task ListAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = await service.GetAsync(token);

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
        }
    }
}